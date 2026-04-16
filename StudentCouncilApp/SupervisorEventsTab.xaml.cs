using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class SupervisorEventsTab : UserControl
    {
        private int _studentId;
        private DatabaseHelper _db;
        private List<Event> _events;
        private List<Direction> _myDirections;
        private int _editingEventId = 0;

        public SupervisorEventsTab(int studentId, DatabaseHelper db, List<Event> events, List<Direction> myDirections)
        {
            InitializeComponent();
            _studentId = studentId;
            _db = db;
            _events = events;
            _myDirections = myDirections;

            LoadEvents();
        }

        private void LoadEvents()
        {
            var eventViewModels = new List<EventViewModel>();

            foreach (var ev in _events)
            {
                var direction = _myDirections.FirstOrDefault(d => d.DirectionID == ev.DirectionID);
                var participantsCount = _db.GetEventParticipantsCount(ev.EventID);

                eventViewModels.Add(new EventViewModel
                {
                    EventID = ev.EventID,
                    Name = ev.Name,
                    DirectionName = direction?.Name ?? "Неизвестно",
                    FormattedDateTime = $"{ev.EventDate:dd.MM.yyyy} в {ev.EventTime:hh\\:mm}",
                    Place = ev.Place,
                    Scores = ev.Scores ?? 0,
                    ParticipantsCount = participantsCount
                });
            }

            EventsList.ItemsSource = eventViewModels;
        }

        private void BtnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            _editingEventId = 0;
            dialogTitle.Text = "Добавить мероприятие";
            ClearDialog();

            cmbDirection.ItemsSource = _myDirections;
            cmbDirection.SelectedIndex = 0;

            dateEvent.SelectedDate = DateTime.Now.AddDays(7);

            eventDialog.Visibility = Visibility.Visible;
        }

        private void BtnEditEvent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int eventId = (int)button.Tag;
            var ev = _db.GetEventById(eventId);

            if (ev != null)
            {
                _editingEventId = eventId;
                dialogTitle.Text = "Редактировать мероприятие";

                txtName.Text = ev.Name;
                cmbDirection.ItemsSource = _myDirections;
                cmbDirection.SelectedValue = ev.DirectionID;
                dateEvent.SelectedDate = ev.EventDate;
                txtTime.Text = ev.EventTime.ToString(@"hh\:mm");
                txtPlace.Text = ev.Place;
                txtScores.Text = ev.Scores.ToString();
                txtExpected.Text = ev.ExpectedStudentsAmount.ToString();

                eventDialog.Visibility = Visibility.Visible;
            }
        }

        private async void BtnDeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int eventId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить это мероприятие?\nВсе заявки и записи будут удалены.",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _db.DeleteEvent(eventId);
                if (success)
                {
                    MessageBox.Show("Мероприятие удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshEvents();
                }
                else
                {
                    MessageBox.Show("Ошибка удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveEvent_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbDirection.SelectedItem == null)
            {
                MessageBox.Show("Выберите направление!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dateEvent.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(txtTime.Text, out TimeSpan eventTime))
            {
                MessageBox.Show("Введите время в формате ЧЧ:ММ (например 14:30)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtScores.Text, out int scores))
            {
                MessageBox.Show("Введите корректное количество баллов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtExpected.Text, out int expected))
            {
                MessageBox.Show("Введите корректное количество участников!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedDirection = cmbDirection.SelectedItem as Direction;
            var studyPeriods = _db.GetStudyPeriods();
            var currentPeriod = studyPeriods.FirstOrDefault(p => p.DateStart <= DateTime.Now && p.DateEnd >= DateTime.Now);

            var ev = new Event
            {
                Name = txtName.Text,
                DirectionID = selectedDirection.DirectionID,
                EventDate = dateEvent.SelectedDate.Value,
                EventTime = eventTime,
                Place = txtPlace.Text,
                Scores = scores,
                ExpectedStudentsAmount = expected,
                ActualStudentsAmount = 0,
                StudyPeriodID = currentPeriod?.PeriodID ?? 1
            };

            bool success;

            if (_editingEventId == 0)
            {
                success = _db.AddEvent(ev);
                if (success)
                    MessageBox.Show("Мероприятие добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ev.EventID = _editingEventId;
                success = _db.UpdateEvent(ev);
                if (success)
                    MessageBox.Show("Мероприятие обновлено!\nУчастники уведомлены об изменениях.",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (success)
            {
                eventDialog.Visibility = Visibility.Collapsed;
                RefreshEvents();
            }
            else
            {
                MessageBox.Show("Ошибка сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelDialog_Click(object sender, RoutedEventArgs e)
        {
            eventDialog.Visibility = Visibility.Collapsed;
        }

        private void ClearDialog()
        {
            txtName.Text = "";
            txtTime.Text = "14:00";
            txtPlace.Text = "";
            txtScores.Text = "10";
            txtExpected.Text = "30";
            dateEvent.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void RefreshEvents()
        {
            _events = _db.GetEventsBySupervisor(_studentId);
            LoadEvents();
        }
    }

    // Вспомогательный класс для отображения
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public string DirectionName { get; set; }
        public string FormattedDateTime { get; set; }
        public string Place { get; set; }
        public int Scores { get; set; }
        public int ParticipantsCount { get; set; }
    }
}