using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Reloadify.Forms
{
    public partial class HotReload
    {
		private static readonly WeakList<VisualElement> activeViews = new WeakList<VisualElement>();

		private static Dictionary<string, Type> replacedViews = new Dictionary<string, Type>();
		private static Dictionary<string, Type> replacedBindingContext = new Dictionary<string, Type>();
		private static Dictionary<object, object[]> bindingContextMap = new Dictionary<object, object[]>();
		private static Dictionary<VisualElement, object[]> currentViews = new Dictionary<VisualElement, object[]>();

		/// <summary>
		/// Returns True if HotReload is Enabled
		/// </summary>
		public static bool IsEnabled { get; set; } = Debugger.IsAttached;

		/// <summary>
		/// Register the HotReload to watch a Xamarin.Forms project
		/// </summary>
		/// <param name="ideIp"></param>
		/// <param name="idePort"></param>
		public static void Register(string ideIp = null, int idePort = 9988)
		{
			Reload.Instance.FinishedReload = TriggerReload;

			Reload.Instance.ReplaceType = (d) =>
				RegisterReplacedType(d.ClassName, d.Type);

			_ = Task.Run(async () =>
			{
				try
				{
					var success = await Reload.Init(ideIp, idePort).ConfigureAwait(false);
					Console.WriteLine($"### HotReload Initialize: {success}");
				}
				catch (Exception ex)
				{
					LogHotReloadError(ex, "### HotReload was not initialized!");
				}
			});
		}

        private static void Application_PageAppearing(object sender, Page e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Register a ViewsualElement to observe changes
        /// </summary>
        /// <param name="visualElement">Element to observe</param>
        public static void WatchInstance(VisualElement visualElement)
		{
			if (replacedViews.Any(r => r.Value == visualElement.GetType()))
				return;

			if (activeViews.Any(v => v == visualElement))
				return;

			activeViews.Add(visualElement);
			Debug.WriteLine($"Active View Count: {activeViews.Count}");
		}

		/// <summary>
		/// Register a custom constructor with parameters
		/// </summary>
		/// <param name="view"></param>
		/// <param name="parameters"></param>
		public static void Register(VisualElement view, params object[] parameters)
		{
			if (!IsEnabled)
				return;

			currentViews[view] = parameters;
		}

		/// <summary>
		/// Unregister a VisualElement
		/// </summary>
		/// <param name="view"></param>
		public static void UnRegister(VisualElement view)
		{
			if (!IsEnabled)
				return;

			_ = currentViews.Remove(view);
		}

		/// <summary>
		/// Reset the HotReload observer
		/// </summary>
        public static void Reset() =>
			replacedViews.Clear();

		/// <summary>
		/// Trigger all know context`s to reload
		/// </summary>
        private static void TriggerReload()
		{
			TriggerForBindingContext();
			TriggerForViews();
		}

		/// <summary>
		/// Prints the error message on output
		/// </summary>
		/// <param name="ex">Current exception</param>
		/// <param name="message">Detailed message</param>
		private static void LogHotReloadError(Exception ex, string message = "### Error on replacing views for HotReload C#")
		{
			Console.WriteLine(message);
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
		}
	}
}
