using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;
using StudentCouncilApp.Models;

namespace StudentCouncilApp
{
    public partial class EventsPage : UserControl
    {
        private int _studentId;
        private DatabaseHelper _db;
        private List<EventDisplayModel> _allEvents;
        private int _currentEventId;
        private bool _isInitialized = false;  // Флаг инициализации

        public EventsPage(int studentId, DatabaseHelper db)
        {
            InitializeComponent();

            _studentId = studentId;
            _db = db;

            // Подписываемся на события ТОЛЬКО после инициализации
            SubscribeFilters();

            // Загружаем мероприятия
            LoadEvents();

            _isInitialized = true;
        }

        private void SubscribeFilters()
        {
            // Отписываемся сначала (на всякий случай)
            if (filterAll != null)
                filterAll.Checked -= Filter_Checked;
            if (filter1 != null)
                filter1.Checked -= Filter_Checked;
            if (filter2 != null)
                filter2.Checked -= Filter_Checked;
            if (filter3 != null)
                filter3.Checked -= Filter_Checked;
            if (filter4 != null)
                filter4.Checked -= Filter_Checked;
            if (filter5 != null)
                filter5.Checked -= Filter_Checked;
            if (filter6 != null)
                filter6.Checked -= Filter_Checked;
            if (filter7 != null)
                filter7.Checked -= Filter_Checked;

            // Подписываемся заново
            if (filterAll != null)
                filterAll.Checked += Filter_Checked;
            if (filter1 != null)
                filter1.Checked += Filter_Checked;
            if (filter2 != null)
                filter2.Checked += Filter_Checked;
            if (filter3 != null)
                filter3.Checked += Filter_Checked;
            if (filter4 != null)
                filter4.Checked += Filter_Checked;
            if (filter5 != null)
                filter5.Checked += Filter_Checked;
            if (filter6 != null)
                filter6.Checked += Filter_Checked;
            if (filter7 != null)
                filter7.Checked += Filter_Checked;
        }

        private void LoadEvents(int? directionId = null)
        {
            try
            {
                // Проверка, что _db не null
                if (_db == null)
                {
                    // Просто выходим, без сообщения
                    return;
                }

                var events = _db.GetEventsByDirection(directionId);

                if (events == null)
                    events = new List<Event>();

                var studentRequests = _db.GetStudentRequests(_studentId);
                if (studentRequests == null)
                    studentRequests = new List<ParticipationRequest>();

                var studentParticipations = _db.GetStudentParticipations(_studentId);
                if (studentParticipations == null)
                    studentParticipations = new List<StudentParticipation>();

                _allEvents = new List<EventDisplayModel>();

                foreach (var ev in events)
                {
                    var status = GetEventStatus(ev.EventID, studentRequests, studentParticipations);
                    var participantsCount = _db.GetEventParticipantsCount(ev.EventID);

                    var displayEvent = new EventDisplayModel
                    {
                        EventID = ev.EventID,
                        Name = ev.Name,
                        EventDate = ev.EventDate,
                        EventTime = ev.EventTime,
                        Place = ev.Place,
                        Scores = ev.Scores ?? 0,
                        ExpectedStudentsAmount = ev.ExpectedStudentsAmount ?? 0,
                        ActualStudentsAmount = ev.ActualStudentsAmount ?? 0,
                        DirectionName = _db.GetDirectionName(ev.DirectionID),
                        DirectionID = ev.DirectionID,
                        Status = status,
                        ParticipantsCount = participantsCount
                    };

                    _allEvents.Add(displayEvent);
                }

                if (EventsList != null)
                    EventsList.ItemsSource = _allEvents;
            }
            catch (Exception ex)
            {
                // Логируем ошибку но не показываем пользователю
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки мероприятий: {ex.Message}");
            }
        }

        private string GetEventStatus(int eventId,
                                       List<ParticipationRequest> requests,
                                       List<StudentParticipation> participations)
        {
            if (participations != null && participations.Any(p => p.EventID == eventId))
                return "✓ Участвую";

            if (requests != null && requests.Any(r => r.EventID == eventId))
                return "Заявка отправлена";

            return "Хочу принять участие";
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Игнорируем вызовы до полной инициализации
            if (!_isInitialized || _db == null) return;

            try
            {
                var radio = sender as RadioButton;

                if (radio == null) return;

                if (radio == filterAll)
                {
                    LoadEvents();
                }
                else if (radio.Tag != null)
                {
                    int directionId = int.Parse(radio.Tag.ToString());
                    LoadEvents(directionId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private async void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null) return;

            try
            {
                var button = sender as Button;
                if (button == null) return;

                int eventId = (int)button.Tag;
                var currentEvent = _allEvents?.FirstOrDefault(ev => ev.EventID == eventId);

                if (currentEvent == null) return;

                if (currentEvent.Status == "Хочу принять участие")
                {
                    var result = MessageBox.Show($"Вы хотите принять участие в мероприятии:\n\n{currentEvent.Name}\n\n{currentEvent.FormattedDate}\n{currentEvent.Place}\n\nБонус: +{currentEvent.Scores} баллов",
                                                 "Запись на мероприятие",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _db.AddParticipationRequest(_studentId, eventId, "Хочу участвовать!");

                        if (success)
                        {
                            MessageBox.Show("Заявка на участие отправлена! Руководитель направления рассмотрит её.",
                                           "Успешно",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Information);
                            LoadEvents();
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
                else
                {
                    MessageBox.Show($"Вы уже {currentEvent.Status.ToLower()} на это мероприятие.\n\nСтатус изменить нельзя.",
                                   "Информация",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void BtnViewParticipants_Click(object sender, RoutedEventArgs e)
        {
            if (_db == null) return;

            try
            {
                var button = sender as Button;
                if (button == null) return;

                int eventId = (int)button.Tag;
                var currentEvent = _allEvents?.FirstOrDefault(ev => ev.EventID == eventId);

                if (currentEvent != null)
                {
                    ShowParticipants(eventId, currentEvent.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void ShowParticipants(int eventId, string eventName)
        {
            try
            {
                var participants = _db.GetEventParticipants(eventId);
                var participantsWithDetails = new List<ParticipantInfo>();

                foreach (var p in participants)
                {
                    var group = _db.GetStudentGroup(p.GroupID);
                    participantsWithDetails.Add(new ParticipantInfo
                    {
                        FullName = $"{p.FName} {p.LName}",
                        GroupName = group?.Name ?? "Группа не указана"
                    });
                }

                if (participantsEventTitle != null)
                    participantsEventTitle.Text = $"👥 {eventName}";
                if (participantsCount != null)
                    participantsCount.Text = $"{participants.Count} участников";
                if (participantsList != null)
                    participantsList.ItemsSource = participantsWithDetails;
                if (participantsOverlay != null)
                    participantsOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки участников: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void CloseParticipants_Click(object sender, RoutedEventArgs e)
        {
            if (participantsOverlay != null)
                participantsOverlay.Visibility = Visibility.Collapsed;
        }
    }

    // Вспомогательный класс для отображения участников
    public class ParticipantInfo
    {
        public string FullName { get; set; }
        public string GroupName { get; set; }
    }
}