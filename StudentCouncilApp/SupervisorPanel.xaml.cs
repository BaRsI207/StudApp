using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class SupervisorPanel : UserControl
    {
        private int _studentId;
        private DatabaseHelper _db;
        private List<Direction> _myDirections;
        private bool _isInitialized = false;  // Флаг инициализации

        public SupervisorPanel(int studentId, DatabaseHelper db)
        {
            InitializeComponent();

            _studentId = studentId;
            _db = db;

            _myDirections = _db.GetStudentDirections(studentId);
            string directions = string.Join(", ", _myDirections.Select(d => d.Name));
            txtSupervisorInfo.Text = $"Руководитель: {directions}";

            // Подписываемся на события ТОЛЬКО после инициализации
            SubscribeTabs();

            // Загружаем первую вкладку
            LoadEventsTab();

            _isInitialized = true;
        }

        private void SubscribeTabs()
        {
            // Отписываемся сначала (на всякий случай)
            tabEvents.Checked -= TabEvents_Checked;
            tabRequests.Checked -= TabRequests_Checked;
            tabDirection.Checked -= TabDirection_Checked;
            tabReports.Checked -= TabReports_Checked;

            // Подписываемся заново
            tabEvents.Checked += TabEvents_Checked;
            tabRequests.Checked += TabRequests_Checked;
            tabDirection.Checked += TabDirection_Checked;
            tabReports.Checked += TabReports_Checked;
        }

        private void TabEvents_Checked(object sender, RoutedEventArgs e)
        {
            // Игнорируем вызовы до полной инициализации
            if (!_isInitialized || _db == null) return;
            LoadEventsTab();
        }

        private void TabRequests_Checked(object sender, RoutedEventArgs e)
        {
            // Игнорируем вызовы до полной инициализации
            if (!_isInitialized || _db == null) return;
            LoadRequestsTab();
        }

        private void TabDirection_Checked(object sender, RoutedEventArgs e)
        {
            // Игнорируем вызовы до полной инициализации
            if (!_isInitialized || _db == null) return;
            LoadDirectionTab();
        }

        private void TabReports_Checked(object sender, RoutedEventArgs e)
        {
            // Игнорируем вызовы до полной инициализации
            if (!_isInitialized || _db == null) return;
            LoadReportsTab();
        }

        private void LoadEventsTab()
        {
            try
            {
                if (_db == null) return;

                var events = _db.GetEventsBySupervisor(_studentId);
                var eventsTab = new SupervisorEventsTab(_studentId, _db, events, _myDirections);
                tabContent.Content = eventsTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки вкладки мероприятий: {ex.Message}");
            }
        }

        private void LoadRequestsTab()
        {
            try
            {
                if (_db == null) return;

                var events = _db.GetEventsBySupervisor(_studentId);
                var requestsTab = new SupervisorRequestsTab(_db, events);
                tabContent.Content = requestsTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки вкладки заявок: {ex.Message}");
            }
        }

        private void LoadDirectionTab()
        {
            try
            {
                if (_db == null || _myDirections == null || !_myDirections.Any()) return;

                var directionTab = new SupervisorDirectionTab(_db, _myDirections.FirstOrDefault());
                tabContent.Content = directionTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки вкладки направления: {ex.Message}");
            }
        }

        private void LoadReportsTab()
        {
            try
            {
                if (_db == null) return;

                var reportsTab = new SupervisorReportsTab(_db, _myDirections);
                tabContent.Content = reportsTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки вкладки отчетности: {ex.Message}");
            }
        }
    }
}