using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

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

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Login));
        }

    }
}
