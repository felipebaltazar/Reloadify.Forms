using System;

namespace ReloadfyForms
{
    public class MainPageViewModel : ObservableObject
    {
        private string text = "Hello from viewmodel";
        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public MainPageViewModel() : base()
        {
        }
    }
}
