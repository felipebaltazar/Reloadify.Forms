﻿
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReloadfyForms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SecondPage : ContentPage
    {
        public SecondPage()
        {
            InitializeComponent();
            this.Title = "Second Page";
        }
    }
}