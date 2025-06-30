namespace CouchbaseTodo
{
    public partial class MainPage : ContentPage
    {
        public MainPage(ViewModels.MainPageViewModel viewModel)
        {
            BindingContext = viewModel;
            InitializeComponent();
        }

    }
}
