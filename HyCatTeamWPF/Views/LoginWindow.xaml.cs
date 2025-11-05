using HyCatTeamWPF.ApiServices;
using HyCatTeamWPF.Helpers;
using HyCatTeamWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HyCatTeamWPF
{
   
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthApiService _authService;
        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthApiService(ApiClient.Client);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = "";

            var req = new UserLoginReq
            {
                Email = TxtEmail.Text,
                Password = TxtPassword.Password
            };

            var token = await _authService.LoginAsync(req);

            if (token == null)
            {
                TxtStatus.Text = "Incorrect Email Or Passoword!";
                return;
            }

            // Lưu access token
            TokenStorage.SaveAccessToken(token);

            // Gắn token vào HttpClient để authorize
            ApiClient.AttachToken();

            MessageBox.Show("Login Success!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Mở main window
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
