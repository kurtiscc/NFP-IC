using NFP_IC.Models;
using NFP_IC.Utils;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace NFP_IC.Views
{
    public sealed partial class Welcome : Page
    {
        private Account _activeAccount;

        public Welcome()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _activeAccount = (Account)e.Parameter;
            if (_activeAccount != null)
            {
                UserNameText.Text = _activeAccount.Username;
            }
        }

        private void Button_Restart_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserSelection));
        }

        private void Button_Forget_User_Click(object sender, RoutedEventArgs e)
        {
            // Remove it from Microsoft Passport
            MicrosoftPassportHelper.RemovePassportAccountAsync(_activeAccount);

            // Remove it from the local accounts list and resave the updated list
            AccountHelper.RemoveAccount(_activeAccount);

            Debug.WriteLine("User " + _activeAccount.Username + " deleted.");

            // Navigate back to UserSelection page.
            Frame.Navigate(typeof(UserSelection));
        }
    }
}
