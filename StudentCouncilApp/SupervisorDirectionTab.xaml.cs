using System;
using System.Windows;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class SupervisorDirectionTab : UserControl
    {
        private DatabaseHelper _db;
        private Direction _direction;

        public SupervisorDirectionTab(DatabaseHelper db, Direction direction)
        {
            InitializeComponent();
            _db = db;
            _direction = direction;

            LoadDirectionData();
        }

        private void LoadDirectionData()
        {
            if (_direction != null)
            {
                txtDirectionName.Text = _direction.Name;
                txtDirectionNote.Text = _direction.Note ?? "";

                // Загружаем приветственное сообщение из БД (если есть поле, или храним отдельно)
                // Пока используем заглушку
                txtWelcomeMessage.Text = GetWelcomeMessage();
            }
        }

        private string GetWelcomeMessage()
        {
            // Здесь можно загружать из отдельной таблицы или поля
            // Пока возвращаем пример
            var student = _db.GetStudentById(1); // Временное решение
            return $"Привет! Я {student?.FName} {student?.LName}, руководитель направления. Присоединяйся к нашей команде!";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_direction == null)
            {
                MessageBox.Show("Направление не найдено!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = _db.UpdateDirectionInfo(_direction.DirectionID, txtDirectionName.Text, txtDirectionNote.Text);

            if (success)
            {
                txtStatus.Text = "✓ Информация о направлении успешно обновлена!";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtStatus.Visibility = Visibility.Visible;

                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (s, args) =>
                {
                    txtStatus.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
            else
            {
                txtStatus.Text = "✗ Ошибка сохранения!";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtStatus.Visibility = Visibility.Visible;
            }
        }
    }
}