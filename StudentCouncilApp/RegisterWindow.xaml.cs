using System;
using System.Linq;
using System.Windows;
using StudentCouncilApp.Data;
using StudentCouncilApp.Security;

namespace StudentCouncilApp
{
    public partial class RegisterWindow : Window
    {
        private DatabaseHelper _db;

        public RegisterWindow(DatabaseHelper db)
        {
            InitializeComponent();
            _db = db;
            LoadGroups();

            btnRegister.Click += BtnRegister_Click;
            btnCancel.Click += (s, e) => this.Close();
        }

        private void LoadGroups()
        {
            var groups = _db.GetAllGroups();
            cmbGroup.ItemsSource = groups;
            if (groups.Any())
                cmbGroup.SelectedIndex = 0;
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения
            if (string.IsNullOrWhiteSpace(txtLName.Text) ||
                string.IsNullOrWhiteSpace(txtFName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                txtStatus.Text = "Заполните все поля, отмеченные *";
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                txtStatus.Text = "Пароли не совпадают";
                return;
            }

            if (cmbGroup.SelectedItem == null)
            {
                txtStatus.Text = "Выберите группу";
                return;
            }

            dynamic selectedGroup = cmbGroup.SelectedItem;
            int groupId = selectedGroup.GroupID;

            string passwordHash = PasswordHasher.HashPassword(txtPassword.Password);

            bool success = _db.Register(
                txtLogin.Text,
                passwordHash,
                txtLName.Text,
                txtFName.Text,
                txtMName.Text,
                txtEmail.Text,
                txtPhone.Text,
                groupId
            );

            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                txtStatus.Text = "Пользователь с таким логином уже существует";
            }
        }
    }
}