using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MMOCommon;
using MatrixAPI.Encryption;

namespace MMOTestClient
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private static bool hasReturned = false;
        private static LoginRequest req;
        private static string salt;
        public LoginForm()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            hasReturned = true;
            DialogResult = true;
            req = new LoginRequest()
            {
                MD5Pass =
                    StringMD5.CreateMD5Hash(
                        StringMD5.CreateMD5Hash(salt + passwordBox.Text) + salt),
                Username = usernameBox.Text
            };
            Close();
        }

        public static LoginRequest CreateRequest(string tsalt)
        {
            LoginForm window = null;
            salt = tsalt;
            hasReturned = false;
            Thread loginThread = new Thread(() =>
                                                {
                                                    window = new LoginForm();
                                                    window.ShowDialog();
                                                });
            loginThread.SetApartmentState(ApartmentState.STA);
            loginThread.Start();

            while(!hasReturned)
            {
                Thread.Sleep(50);
            }

            return req;
        }
    }
}
