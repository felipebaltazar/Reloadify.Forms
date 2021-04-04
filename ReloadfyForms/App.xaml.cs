using Reloadify.Forms;
using Xamarin.Forms;

namespace ReloadfyForms
{
    public partial class App : Application
    {
        public App()
        {
            HotReload.Register();
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
	}
}
