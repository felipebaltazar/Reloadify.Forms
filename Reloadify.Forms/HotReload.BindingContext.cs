using Reloadify.Forms.Extensions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace Reloadify.Forms
{
    public partial class HotReload
    {
		/// <summary>
		/// Update all binding context changes
		/// </summary>
		private static void TriggerForBindingContext()
		{
			var bindingContexts = replacedBindingContext.ToList();

			if (bindingContexts.IsNullOrEmpty())
				return;

			Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var bindingContext in bindingContexts)
				{
					try
					{
						var oldType = bindingContext.Key;
						var newType = bindingContext.Value;

						var viewToProcess = activeViews.FirstOrDefault(v => v.BindingContext.GetType().FullName == oldType);
						if (viewToProcess is null)
							continue;

						var newBindingContext = GetReplacedBindingContext(viewToProcess);
						if (newBindingContext is null)
							continue;

						viewToProcess.BindingContext = newBindingContext;
					}
					catch (Exception ex)
					{
						LogHotReloadError(ex);
					}
				}
			});
		}

		/// <summary>
		/// Register a new changed object
		/// </summary>
		/// <param name="oldType"></param>
		/// <param name="newType"></param>
		public static void RegisterReplacedType(string oldType, Type newType)
		{
			try
			{
				if (!IsEnabled)
					return;

				Console.WriteLine($"{oldType} - {newType}");

				if (newType.IsSubclassOf(typeof(VisualElement)))
				{
					if (replacedViews.ContainsKey(oldType))
						replacedViews[oldType] = newType;
					else
						replacedViews.Add(oldType, newType);
				}
				else if (typeof(INotifyPropertyChanged).IsAssignableFrom(newType))
				{
					if (replacedBindingContext.ContainsKey(oldType))
						replacedBindingContext[oldType] = newType;
					else
						replacedBindingContext.Add(oldType, newType);
				}

				//Call static init if it exists on new classes!
				var staticInit = newType.GetMethod("Init", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				staticInit?.Invoke(null, null);
			}
			catch (Exception ex)
			{
				LogHotReloadError(ex);
			}
		}

		/// <summary>
		/// Get a new replaced binding context
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static object GetReplacedBindingContext(VisualElement view)
		{
			if (!IsEnabled || view?.BindingContext is null)
				return view?.BindingContext;

			if (!replacedBindingContext.TryGetValue(view.BindingContext?.GetType().FullName, out var newType))
				return view?.BindingContext;

			_ = bindingContextMap.TryGetValue(view, out var parameters);

			object newObject = null;

			try
			{
				newObject = (parameters?.Length > 0
					? Activator.CreateInstance(newType, args: parameters) : Activator.CreateInstance(newType));

				return newObject;
			}
			catch (MissingMethodException ex)
			{
				Debug.WriteLine("You are using Reloadify.Forms on a view that requires Parameters. Please call `HotReloadHelper.Register(this, params);` in the constructor;");
				throw ex;
			}
			finally
			{
				if (newObject != null)
					RemoveReplacedBindingContext(view, newObject);
			}
		}

		private static void RemoveReplacedBindingContext(VisualElement view, object newObject)
		{
			try
			{
				_ = replacedBindingContext.Remove(view.BindingContext?.GetType().FullName);

				if (newObject is IDisposable disposable)
					disposable.Dispose();
			}
			catch (Exception ex)
			{
				LogHotReloadError(ex);
			}
		}
	}
}
