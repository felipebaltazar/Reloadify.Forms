using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Reloadify.Forms
{
    public class HotReload
    {
		private static readonly WeakList<VisualElement> activeViews = new WeakList<VisualElement>();
		private static Dictionary<string, Type> replacedViews = new Dictionary<string, Type>();
		private static Dictionary<string, Type> replacedBindingContext = new Dictionary<string, Type>();
		private static Dictionary<object, object[]> bindingContextMap = new Dictionary<object, object[]>();
		private static Dictionary<VisualElement, object[]> currentViews = new Dictionary<VisualElement, object[]>();

		public static void Enable(string ideIp = null, int idePort = 9988)
		{
			//InternalInit();

			Reload.Instance.ReplaceType = (d) => {
				RegisterReplacedType(d.ClassName, d.Type);
			};

			Reload.Instance.FinishedReload = () => {
				TriggerReload();
			};

			_ = Task.Run(async () =>
			{
				try
				{
					var success = await Reload.Init(ideIp, idePort);
					Console.WriteLine($"HotReload Initialize: {success}");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			});
		}

		public static void InsertInstance(VisualElement visualElement)
		{
			if (replacedViews.Any(r => r.Value == visualElement.GetType()))
				return;

			activeViews.Add(visualElement);
			Debug.WriteLine($"Active View Count: {activeViews.Count}");
		}

		public static void Reset()
		{
			replacedViews.Clear();
		}

		public static bool IsEnabled { get; set; } = Debugger.IsAttached;

		public static void Register(VisualElement view, params object[] parameters)
		{
			if (!IsEnabled)
				return;

			currentViews[view] = parameters;
		}

		public static void UnRegister(VisualElement view)
		{
			if (!IsEnabled)
				return;

			currentViews.Remove(view);
		}

		public static bool IsReplacedView(VisualElement view, VisualElement newView)
		{
			if (!IsEnabled)
				return false;

			if (view == null || newView == null)
				return false;

			if (!replacedViews.TryGetValue(view.GetType().FullName, out var newViewType))
				return false;

			return newView.GetType() == newViewType;
		}

		public static VisualElement GetReplacedView(VisualElement view)
		{
			if (!IsEnabled || view is null)
				return view;

			if (!replacedViews.TryGetValue(view.GetType().FullName, out var newViewType))
				return view;

			currentViews.TryGetValue(view, out var parameters);

			try
			{
				var newView = (VisualElement)(parameters?.Length > 0 ? Activator.CreateInstance(newViewType, args: parameters) : Activator.CreateInstance(newViewType));
				TransferState(view, newView);
				return newView;
			}
			catch (MissingMethodException ex)
			{
				Debug.WriteLine("You are using Reloadify on a view that requires Parameters. Please call `Hotreload.Register(this, params);` in the constructor;");
				throw ex;
			}
            finally
            {
				_ = replacedViews.Remove(view.GetType().FullName);

			}
		}

		public static object GetReplacedBindingContext(VisualElement view)
		{
			if (!IsEnabled || view?.BindingContext is null)
				return view?.BindingContext;

			if (!replacedBindingContext.TryGetValue(view.BindingContext?.GetType().FullName, out var newType))
				return view?.BindingContext;

			_ = bindingContextMap.TryGetValue(view, out var parameters);

			try
			{
				var newObject = (parameters?.Length > 0
					? Activator.CreateInstance(newType, args: parameters) : Activator.CreateInstance(newType));

				return newObject;
			}
			catch (MissingMethodException ex)
			{
				Debug.WriteLine("You are using Comet.Reload on a view that requires Parameters. Please call `HotReloadHelper.Register(this, params);` in the constructor;");
				throw ex;
			}
            finally
            {
				_ = replacedBindingContext.Remove(view.BindingContext?.GetType().FullName);
			}
		}

		static void TransferState(VisualElement oldView, VisualElement newView)
		{
			var oldState = oldView?.BindingContext;
			newView.BindingContext = oldState;
		}

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
				Console.WriteLine("### Error on replacing views for HotReload C#");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		public static void TriggerReload()
		{
			TriggerForBindingContext();
			TriggerForViews();
		}

		private static void TriggerForBindingContext()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var bindingContext in replacedBindingContext)
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
						Console.WriteLine("### Error on replacing views for HotReload C#");
						Console.WriteLine(ex.Message);
						Console.WriteLine(ex.StackTrace);
					}
				}
			});
		}

		private static void TriggerForViews()
		{
			List<VisualElement> roots = null;
			while (roots == null)
			{
				try
				{
					roots = activeViews.Where(v=> v != null).ToList();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

            Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var view in roots)
				{
                    try
                    {
						var replaced = GetReplacedView(view);
						if (replaced is null || view == replaced)
							continue;

						var parent = view?.Parent ?? replaced?.Parent;
						if(parent is Application application)
						{
							application.MainPage = replaced as Page;
						}
						else if (parent is NavigationPage navPage)
						{
							var useModalStack = false;
							var pageIndex = navPage.Navigation.NavigationStack.IndexOf(p => p == view);
							if(pageIndex < 0)
							{
								pageIndex = navPage.Navigation.ModalStack.IndexOf(p => p == view);
								useModalStack = true;
							}

							ResolveForNavigation(navPage.Navigation, view, replaced, pageIndex, useModalStack);
						}

						activeViews.Replace(view, replaced);
                    }
					catch (Exception ex)
                    {
						Console.WriteLine("### Error on replacing views for HotReload C#");
						Console.WriteLine(ex.Message);
						Console.WriteLine(ex.StackTrace);
					}
                }
			});
		}

        private static void ResolveForNavigation(INavigation navigation, VisualElement view, VisualElement replaced, int pageIndex, bool useModalStack)
        {
			navigation.InsertPageBefore(replaced as Page, view as Page);
			var lastIndex = (useModalStack
				? navigation.NavigationStack.Count
				: navigation.NavigationStack.Count) - 1;


			if (pageIndex == lastIndex)
				if (useModalStack)
					_ = navigation.PopModalAsync();
				else
					_ = navigation.PopAsync();
			else
				navigation.RemovePage(view as Page);
        }
    }
}
