using Xamarin.Forms;

namespace Reloadify.Forms
{
    public class InstanceWatcher
    {
        public InstanceWatcher(object objectInstance)
        {
            if (objectInstance is VisualElement visualElement)
            {
                HotReload.InsertInstance(visualElement);
            }
        }
    }
}
