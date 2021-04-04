using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace ReloadfyForms
{
    public class MainPageViewModel : ObservableObject
    {
        private int counter;
        private string text = "Hello from viewmodel";
        private Color backgroundColor = Color.Fuchsia;

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set => SetProperty(ref backgroundColor, value);
        }

        public ICommand OnButtonClickCommand { get; }

        public MainPageViewModel() : base()
        {
            OnButtonClickCommand = new Command(async () => {
                await Application.Current.MainPage.Navigation.PushAsync(new SecondPage());
            });
        }
    }
}
