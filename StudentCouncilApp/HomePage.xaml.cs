using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;
using StudentCouncilApp.Models;

namespace StudentCouncilApp
{
    public partial class HomePage : UserControl
    {
        private int _studentId;
        private DatabaseHelper _db;

        public HomePage(int studentId, DatabaseHelper db)
        {
            InitializeComponent();
            _studentId = studentId;
            _db = db;

            LoadHomePage();
        }

        private void LoadHomePage()
        {
            // Загружаем цитату
            txtQuote.Text = _db.GetRandomQuote();

            // Загружаем статистику енота
            var student = _db.GetStudentById(_studentId);
            var rating = _db.GetStudentRating(_studentId);
            txtRaccoonStats.Text = $"Баллов: {student.Scores} | Место: {rating.place}";

            // Загружаем ближайшее событие
            LoadNearestEvent();

            // Загружаем последние достижения
            var achievements = _db.GetStudentDocuments(_studentId).Take(5).ToList();
            listAchievements.ItemsSource = achievements;
        }

        private void LoadNearestEvent()
        {
            var nearestEvent = _db.GetNearestEvent();

            if (nearestEvent != null)
            {
                txtEventDate.Text = FormatEventDate(nearestEvent.EventDate, nearestEvent.EventTime);
                txtEventName.Text = nearestEvent.Name;
                txtEventPlace.Text = $"📍 {nearestEvent.Place}";
                txtEventScores.Text = $"🎁 Бонус: +{nearestEvent.Scores} баллов";

                int participants = _db.GetEventParticipantsCount(nearestEvent.EventID);
                txtEventParticipants.Text = $"👥 Участников: {participants}";

                // Проверяем, записан ли студент
                bool isRegistered = _db.IsStudentRegisteredToEvent(_studentId, nearestEvent.EventID);
                if (isRegistered)
                {
                    btnJoinEvent.Content = "✓ Вы записаны";
                    btnJoinEvent.Background = System.Windows.Media.Brushes.Gray;
                    btnJoinEvent.IsEnabled = false;
                }

                // Сохраняем ID события в Tag
                btnJoinEvent.Tag = nearestEvent.EventID;
            }
            else
            {
                borderNearestEvent.Visibility = Visibility.Collapsed;
                var noEventBlock = new TextBlock
                {
                    Text = "Пока нет запланированных мероприятий. Загляните позже!",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                var parent = borderNearestEvent.Parent as StackPanel;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(borderNearestEvent);
                    parent.Children.Insert(index, noEventBlock);
                    borderNearestEvent.Visibility = Visibility.Collapsed;
                }
            }
        }

        private string FormatEventDate(DateTime date, TimeSpan time)
        {
            var today = DateTime.Now.Date;
            var eventDate = date.Date;

            if (eventDate == today)
                return $"⏰ Сегодня в {time:hh\\:mm}";
            else if (eventDate == today.AddDays(1))
                return $"⏰ Завтра в {time:hh\\:mm}";
            else if (eventDate == today.AddDays(-1))
                return $"⏰ Вчера в {time:hh\\:mm}";
            else
                return $"📅 {date:dd.MM.yyyy} в {time:hh\\:mm}";
        }

        private void RaccoonWidget_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Находим главное окно и переключаем на рейтинг
            var mainWindow = Window.GetWindow(this) as MainAppWindow;
            if (mainWindow != null)
            {
                mainWindow.SwitchToRating();
            }
        }

        private async void BtnJoinEvent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int eventId = (int)button.Tag;

            var result = MessageBox.Show("Вы хотите принять участие в этом мероприятии?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _db.AddParticipationRequest(_studentId, eventId, "Хочу участвовать!");

                if (success)
                {
                    button.Content = "✓ Заявка отправлена";
                    button.Background = System.Windows.Media.Brushes.Gray;
                    button.IsEnabled = false;

                    MessageBox.Show("Заявка на участие отправлена! Руководитель направления рассмотрит её.",
                                   "Успешно",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Вы уже подавали заявку на это мероприятие.",
                                   "Информация",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
        }
    }
}