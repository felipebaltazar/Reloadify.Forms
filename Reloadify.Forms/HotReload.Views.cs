using Reloadify.Forms.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Reloadify.Forms
{
    public partial class HotReload
    {
		private static void TriggerForViews()
		{
			const int MAX_ATTEMPTS = 3;
			List<VisualElement> views = null;
			var attempt = 0;

			while (views == null && attempt < MAX_ATTEMPTS)
			{
				try
				{
					views = activeViews?.Where(v => v != null)?.ToList();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
                finally
                {
					attempt++;
                }
			}

			if (views.IsNullOrEmpty())
				return;

			Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var view in views)
				{
					try
					{
						var replaced = GetReplacedView(view);
						if (replaced is null || view == replaced)
							continue;

						var parent = view?.Parent ?? replaced?.Parent;
						if (parent is Application application)
						{
							application.MainPage = replaced as Page;
						}
						else if (parent is NavigationPage navPage)
						{
							var useModalStack = false;
							var pageIndex = navPage.Navigation.NavigationStack.IndexOf(p => p == view);
							if (pageIndex < 0)
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
						LogHotReloadError(ex);
					}
				}
			});
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
			VisualElement newView = null;

			try
			{
				//if (HasCodegenAttribute(newViewType, out var xamlPath))
				//{
					//var baseObject = BuildBaseType(newViewType);
					//if (baseObject != null)
     //               {
					//	// Need a way to read xaml
					//	newView = global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(baseObject, xaml);
					//}
				//}
				//else
				//{
					newView = (VisualElement)(parameters?.Length > 0
						? Activator.CreateInstance(newViewType, args: parameters)
						: Activator.CreateInstance(newViewType));
				//}

				if (newView != null)
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
				if (!(newView is null))
					RemoveReplacedView(view);
			}
		}

		private static void RemoveReplacedView(VisualElement view)
		{
			try
			{
				_ = replacedViews.Remove(view.GetType().FullName);

				if (view is IDisposable disposable)
					disposable.Dispose();
			}
			catch (Exception ex)
			{
				LogHotReloadError(ex);
			}
		}

		private static VisualElement BuildBaseType(Type newViewType)
		{
			if (newViewType.IsSubclassOf(typeof(ContentPage)))
				return new ContentPage();
			else if (newViewType.IsSubclassOf(typeof(NavigationPage)))
				return new NavigationPage();

			return null;
		}

		private static void TransferState(VisualElement oldView, VisualElement newView)
		{
			var oldState = oldView?.BindingContext;
			newView.BindingContext = oldState;
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

		private static bool HasCodegenAttribute(Type elementType, out string xamlPath)
        {
			var attr = elementType.GetCustomAttribute<XamlFilePathAttribute>();
			xamlPath = attr?.FilePath;

			return attr != null;
		}
			
	}
}
