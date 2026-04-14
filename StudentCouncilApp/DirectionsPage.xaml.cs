using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StudentCouncilApp.Data;
using StudentCouncilApp.Models;

namespace StudentCouncilApp
{
    public partial class DirectionsPage : UserControl
    {
        private DatabaseHelper _db;
        private List<DirectionModel> _directions;
        private int _currentDirectionId;

        public DirectionsPage(DatabaseHelper db)
        {
            InitializeComponent();
            _db = db;
            LoadDirections();
        }

        private void LoadDirections()
        {
            _directions = new List<DirectionModel>();

            var dbDirections = _db.GetDirections();

            foreach (var dir in dbDirections)
            {
                var supervisor = _db.GetDirectionSupervisor(dir.DirectionID);
                string supervisorName = supervisor != null ? $"{supervisor.FName} {supervisor.LName}" : "Не назначен";

                var direction = new DirectionModel
                {
                    DirectionID = dir.DirectionID,
                    Name = dir.Name,
                    Description = GetDirectionDescription(dir.DirectionID),
                    Icon = GetDirectionIcon(dir.DirectionID),
                    SupervisorName = supervisorName,
                    WelcomeMessage = _db.GetSupervisorWelcomeMessage(dir.DirectionID, supervisorName),
                    Phone = _db.GetSupervisorPhone(dir.DirectionID),
                    TelegramLink = _db.GetDirectionTelegramLink(dir.DirectionID)
                };

                _directions.Add(direction);
            }

            DirectionsGrid.ItemsSource = _directions;
        }

        private string GetDirectionDescription(int directionId)
        {
            var descriptions = new Dictionary<int, string>
            {
                { 1, "Волонтерство, экология, помощь животным и добрые дела" },
                { 2, "Спорт, здоровый образ жизни, соревнования и рекорды" },
                { 3, "Наука, исследования, конференции и интеллект" },
                { 4, "Киберспорт, видеоигры, турниры и тактика" },
                { 5, "Фото, видео, соцсети, TikTok и личный бренд" },
                { 6, "Вечеринки, КВН, творчество и настроение" },
                { 7, "Командообразование, тренинги и лидерство" }
            };

            return descriptions.ContainsKey(directionId) ? descriptions[directionId] : "Активное направление студсовета";
        }

        private string GetDirectionIcon(int directionId)
        {
            var icons = new Dictionary<int, string>
            {
                { 1, "🌍" },
                { 2, "⚽" },
                { 3, "🔬" },
                { 4, "🎮" },
                { 5, "📸" },
                { 6, "🎭" },
                { 7, "👥" }
            };

            return icons.ContainsKey(directionId) ? icons[directionId] : "⭐";
        }

        private void DirectionCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null && border.Tag != null)
            {
                int directionId = (int)border.Tag;
                ShowDirectionDetail(directionId);
            }
        }

        private void ShowDirectionDetail(int directionId)
        {
            var direction = _directions.Find(d => d.DirectionID == directionId);
            if (direction == null) return;

            _currentDirectionId = directionId;

            // Заполняем детали
            detailSupervisorName.Text = direction.SupervisorName;
            detailDirectionName.Text = direction.Name;
            detailWelcomeMessage.Text = direction.WelcomeMessage;

            // Ставим первую букву имени для аватара
            if (!string.IsNullOrEmpty(direction.SupervisorName))
                detailSupervisorInitial.Text = direction.SupervisorName.Substring(0, 1);
            else
                detailSupervisorInitial.Text = "👤";

            // Показываем оверлей
            detailOverlay.Visibility = Visibility.Visible;
        }

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            detailOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnCallSupervisor_Click(object sender, RoutedEventArgs e)
        {
            var direction = _directions.Find(d => d.DirectionID == _currentDirectionId);
            if (direction != null && !string.IsNullOrEmpty(direction.Phone) && direction.Phone != "Нет данных")
            {
                try
                {
                    // Пытаемся открыть WhatsApp или просто показываем номер
                    var phone = direction.Phone.Replace(" ", "").Replace("-", "");
                    Process.Start($"https://wa.me/{phone}");
                }
                catch
                {
                    MessageBox.Show($"Номер руководителя: {direction.Phone}\nВы можете связаться с ним по телефону или через мессенджеры.",
                                   "Контакт руководителя",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Контактные данные руководителя временно недоступны.",
                               "Информация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void BtnJoinChat_Click(object sender, RoutedEventArgs e)
        {
            var direction = _directions.Find(d => d.DirectionID == _currentDirectionId);
            if (direction != null && !string.IsNullOrEmpty(direction.TelegramLink))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = direction.TelegramLink,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show($"Ссылка на беседу: {direction.TelegramLink}\nВы можете вступить по ссылке.",
                                   "Ссылка на беседу",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Ссылка на беседу временно недоступна.",
                               "Информация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }
    }
}