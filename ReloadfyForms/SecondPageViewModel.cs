using System;
using System.Collections.Generic;
using System.Text;

namespace ReloadfyForms
{
    public class SecondPageViewModel : ObservableObject
    {
        private string text = "This is a second page";

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }
    }
}
