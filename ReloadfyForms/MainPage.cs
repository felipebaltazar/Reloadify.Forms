using Xamarin.Forms;

namespace ReloadfyForms
{
    public class MainPage : ContentPage
    {
        public MainPage()
        {
			BuildView();
			BindingContext = new MainPageViewModel();
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

			label.SetBinding(Label.TextProperty, "Text");

			Content = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children =
                {
					label
                }
			};
        }
	}
}
