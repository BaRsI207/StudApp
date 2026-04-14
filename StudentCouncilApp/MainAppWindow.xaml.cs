using StudentCouncilApp.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StudentCouncilApp
{
    public partial class MainAppWindow : Window
    {
        private int _currentStudentId;
        private DatabaseHelper _db;
        private UserControl _currentPage;

        public MainAppWindow(int studentId, DatabaseHelper db)
        {
            InitializeComponent();
            _currentStudentId = studentId;
            _db = db;

            // Подписываемся на кнопки меню
            btnLogout.Click += (s, e) => Logout();
            btnHome.Click += (s, e) => LoadHome();
            btnDirections.Click += (s, e) => LoadDirections();
            btnEvents.Click += (s, e) => LoadEvents();
            btnRating.Click += (s, e) => LoadRating();
            btnProfile.Click += (s, e) => LoadProfile();

            // Загружаем главную по умолчанию
            LoadHome();
        }

        private void Logout()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void LoadHome()
        {
            _currentPage = new HomePage(_currentStudentId, _db);
            ContentArea.Content = _currentPage;
        }

        private void LoadDirections()
        {
            _currentPage = new DirectionsPage(_db);
            ContentArea.Content = _currentPage;
        }

        private void LoadEvents()
        {
            _currentPage = new EventsPage(_currentStudentId, _db);
            ContentArea.Content = _currentPage;
        }

        private void LoadRating()
        {
            _currentPage = new RatingPage(_currentStudentId, _db);
            ContentArea.Content = _currentPage;
        }

        private void LoadProfile()
        {
            _currentPage = new ProfilePage(_currentStudentId, _db);
            ContentArea.Content = _currentPage;
        }

        // Метод для переключения на рейтинг из HomePage
        public void SwitchToRating()
        {
            LoadRating();
        }
    }
}