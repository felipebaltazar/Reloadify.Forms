using Xamarin.Forms;

namespace ReloadfyForms
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            BuildView();
            BindingContext = new MainPageViewModel();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void BuildView()
        {
            var label = new Label()
            {
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.FillAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
            };

            var button = new Button()
            {
                Text = "Clique aqui",
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Silver,
            };

            button.SetBinding(Button.CommandProperty,"OnButtonClickCommand");
            label.SetBinding(Label.TextProperty, "Text");
            this.SetBinding(View.BackgroundColorProperty, "BackgroundColor");

            Content = new StackLayout()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    label,
                    button
                }
            };
        }
    }
}
