using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using NFP_IC.Utils;

namespace NFP_IC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchBrowseView : Page
    {
        public SearchBrowseView()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        // THIS FUNCTION NEEDS TO BE ON THE LANDING PAGE FOR THE APP, IF YOU CHANGE THE LANDING PAGE THEN PLEASE MOVE THE FUNCTION
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load the local Accounts List before navigating to the UserSelection page
            await AccountHelper.LoadAccountListAsync();
            Frame.Navigate(typeof(UserSelection));
        }

    }
}
