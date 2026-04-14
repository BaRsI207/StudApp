using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class ProfilePage : UserControl
    {
        private int _studentId;
        private DatabaseHelper _db;

        public ProfilePage(int studentId, DatabaseHelper db)
        {
            InitializeComponent();
            _studentId = studentId;
            _db = db;

            LoadProfileData();
        }

        private void LoadProfileData()
        {
            var student = _db.GetStudentById(_studentId);
            var group = _db.GetStudentGroup(student.GroupID);

            // Личные данные
            txtFullName.Text = $"{student.FName} {student.LName} {student.MName}".Trim();
            txtGroup.Text = group?.Name ?? "Не указана";
            txtEmail.Text = student.Email;
            txtPhone.Text = student.Phone ?? "";
            txtLogin.Text = student.Login;

            // Загружаем мероприятия
            LoadEvents();

            // Загружаем достижения
            LoadAchievements();
        }

        private void LoadEvents()
        {
            var confirmedEvents = _db.GetStudentRegisteredEvents(_studentId);
            var pendingEvents = _db.GetStudentPendingEvents(_studentId);

            listConfirmedEvents.ItemsSource = confirmedEvents;
            listPendingEvents.ItemsSource = pendingEvents;
        }

        private void LoadAchievements()
        {
            var achievements = _db.GetStudentAchievements(_studentId);
            listAchievements.ItemsSource = achievements;
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            bool success = _db.UpdateStudentProfile(_studentId, txtEmail.Text, txtPhone.Text, txtLogin.Text);

            if (success)
            {
                txtProfileStatus.Text = "✓ Данные успешно сохранены!";
                txtProfileStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtProfileStatus.Visibility = Visibility.Visible;

                // Скрываем сообщение через 3 секунды
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (s, args) =>
                {
                    txtProfileStatus.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
            else
            {
                txtProfileStatus.Text = "✗ Ошибка сохранения данных";
                txtProfileStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtProfileStatus.Visibility = Visibility.Visible;
            }
        }

        private void BtnAddAchievement_Click(object sender, RoutedEventArgs e)
        {
            txtAchievementNote.Text = "";
            txtAchievementLink.Text = "";
            addAchievementOverlay.Visibility = Visibility.Visible;
        }

        private void BtnSaveAchievement_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAchievementNote.Text))
            {
                MessageBox.Show("Введите описание достижения!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _db.AddAchievement(_studentId, txtAchievementLink.Text, txtAchievementNote.Text);

            if (success)
            {
                MessageBox.Show("Достижение добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                addAchievementOverlay.Visibility = Visibility.Collapsed;
                LoadAchievements();
            }
            else
            {
                MessageBox.Show("Ошибка добавления достижения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelAchievement_Click(object sender, RoutedEventArgs e)
        {
            addAchievementOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnDeleteAchievement_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int documentId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить это достижение?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _db.DeleteAchievement(documentId);
                if (success)
                {
                    LoadAchievements();
                }
                else
                {
                    MessageBox.Show("Ошибка удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}