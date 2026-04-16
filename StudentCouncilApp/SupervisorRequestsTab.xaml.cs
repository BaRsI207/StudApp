using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class SupervisorRequestsTab : UserControl
    {
        private DatabaseHelper _db;
        private List<Event> _events;
        private List<RequestViewModel> _allRequests;

        public SupervisorRequestsTab(DatabaseHelper db, List<Event> events)
        {
            InitializeComponent();
            _db = db;
            _events = events;

            LoadEventsFilter();
            LoadRequests();
        }

        private void LoadEventsFilter()
        {
            cmbEventFilter.ItemsSource = _events;
            if (_events.Any())
            {
                cmbEventFilter.SelectedIndex = 0;
            }
        }

        private void LoadRequests(int? eventId = null)
        {
            _allRequests = new List<RequestViewModel>();

            var eventsToShow = eventId.HasValue
                ? _events.Where(e => e.EventID == eventId).ToList()
                : _events;

            foreach (var ev in eventsToShow)
            {
                var requests = _db.GetRequestsByEvent(ev.EventID);

                foreach (var req in requests)
                {
                    var student = _db.GetStudentById(req.StudentID);
                    var group = _db.GetStudentGroup(student.GroupID);

                    _allRequests.Add(new RequestViewModel
                    {
                        RequestID = req.RequestID,
                        StudentName = $"{student.FName} {student.LName}",
                        StudentGroup = group?.Name ?? "Группа не указана",
                        EventName = ev.Name,
                        EventScores = ev.Scores ?? 0,
                        RequestDate = req.RequestDate.HasValue ? req.RequestDate.Value.ToString("dd.MM.yyyy HH:mm") : "Дата неизвестна",
                        Note = req.Note ?? "Без комментария"
                    });
                }
            }

            RequestsList.ItemsSource = _allRequests;

            // Показываем сообщение, если нет заявок
            txtEmptyState.Visibility = _allRequests.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            RequestsList.Visibility = _allRequests.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CmbEventFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEventFilter.SelectedItem != null)
            {
                var selectedEvent = cmbEventFilter.SelectedItem as Event;
                LoadRequests(selectedEvent.EventID);
            }
        }

        private async void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var request = _allRequests.FirstOrDefault(r => r.RequestID == requestId);
            if (request != null)
            {
                var result = MessageBox.Show($"Подтвердить участие студента {request.StudentName}?\n\nБудет начислено {request.EventScores} баллов.",
                                             "Подтверждение",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _db.AcceptRequest(requestId, request.EventScores);

                    if (success)
                    {
                        MessageBox.Show("Участие подтверждено! Баллы начислены студенту.",
                                       "Успех",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Information);
                        RefreshCurrentTab();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка подтверждения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = (int)button.Tag;

            var request = _allRequests.FirstOrDefault(r => r.RequestID == requestId);
            if (request != null)
            {
                var result = MessageBox.Show($"Отклонить заявку студента {request.StudentName}?",
                                             "Подтверждение",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _db.RejectRequest(requestId);

                    if (success)
                    {
                        MessageBox.Show("Заявка отклонена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshCurrentTab();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка отклонения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshCurrentTab()
        {
            if (cmbEventFilter.SelectedItem != null)
            {
                var selectedEvent = cmbEventFilter.SelectedItem as Event;
                LoadRequests(selectedEvent.EventID);
            }
            else
            {
                LoadRequests();
            }
        }
    }

    // Вспомогательный класс для отображения заявок
    public class RequestViewModel
    {
        public int RequestID { get; set; }
        public string StudentName { get; set; }
        public string StudentGroup { get; set; }
        public string EventName { get; set; }
        public int EventScores { get; set; }
        public string RequestDate { get; set; }
        public string Note { get; set; }
    }
}