using StudentCouncilApp.Data;
using System;
using System.Linq;
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

            // Проверяем роль студента
            var student = _db.GetStudentById(studentId);
            if (student.RoleID == 2) // 2 - руководитель
            {
                btnSupervisor.Visibility = Visibility.Visible;
                btnSupervisor.Click += (s, e) => LoadSupervisorPanel();
            }

            // Подписываемся на кнопки меню
            btnLogout.Click += (s, e) => Logout();
            btnHome.Click += (s, e) => LoadHome();
            btnDirections.Click += (s, e) => LoadDirections();
            btnEvents.Click += (s, e) => LoadEvents();
            btnRating.Click += (s, e) => LoadRating();
            btnProfile.Click += (s, e) => LoadProfile();
            btnNotifications.Click += (s, e) => ShowNotifications();

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

        private void LoadSupervisorPanel()
        {
            _currentPage = new SupervisorPanel(_currentStudentId, _db);
            ContentArea.Content = _currentPage;
        }

        private void ShowNotifications()
        {
            var notifications = _db.GetStudentNotifications(_currentStudentId);

            // Исправлено: проверяем IsRead == true вместо !IsRead
            var unreadCount = notifications.Count(n => n.IsRead == false || n.IsRead == null);

            string message = $"📬 Уведомления ({unreadCount} новых):\n\n";

            foreach (var notif in notifications.Take(10))
            {
                message += $"📌 {notif.Title}\n   {notif.Message}\n   📅 {notif.CreatedDate:dd.MM.yyyy HH:mm}\n\n";
            }

            if (!notifications.Any())
                message = "Нет уведомлений";

            MessageBox.Show(message, "Уведомления", MessageBoxButton.OK, MessageBoxImage.Information);

            // Отмечаем все как прочитанные - исправлено
            foreach (var notif in notifications.Where(n => n.IsRead == false || n.IsRead == null))
            {
                _db.MarkNotificationAsRead(notif.NotificationID);
            }
        }
    }
}