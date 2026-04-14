using System;
using System.Windows;
using StudentCouncilApp.Data;
using StudentCouncilApp.Security;

namespace StudentCouncilApp
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper _db;

        public MainWindow()
        {
            InitializeComponent();
            _db = new DatabaseHelper();

            // Подписываемся на события кнопок
            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                txtStatus.Text = "Введите логин и пароль";
                return;
            }

            string passwordHash = PasswordHasher.HashPassword(password);
            int studentId = _db.Authenticate(login, passwordHash);

            if (studentId > 0)
            {
                var student = _db.GetStudentById(studentId);
                txtStatus.Text = $"Добро пожаловать, {student.FName}!";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;

                // Открываем главное окно приложения
                MainAppWindow mainApp = new MainAppWindow(studentId, _db);
                mainApp.Show();
                this.Hide();
            }
            else
            {
                txtStatus.Text = "Неверный логин или пароль";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow(_db);
            registerWindow.ShowDialog();
        }
    }
}