using System;

namespace StudentCouncilApp.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Place { get; set; }
        public int Scores { get; set; }
        public int ActualStudentsAmount { get; set; }
        public string DirectionName { get; set; }

        // Форматированная дата для отображения
        public string FormattedDate
        {
            get
            {
                var today = DateTime.Now.Date;
                var eventDate = EventDate.Date;

                if (eventDate == today)
                    return $"Сегодня, {EventTime:hh\\:mm}";
                else if (eventDate == today.AddDays(1))
                    return $"Завтра, {EventTime:hh\\:mm}";
                else if (eventDate == today.AddDays(-1))
                    return $"Вчера, {EventTime:hh\\:mm}";
                else
                    return $"{EventDate:dd.MM.yyyy}, {EventTime:hh\\:mm}";
            }
        }

        // Статус кнопки
        public string ButtonText { get; set; } = "Хочу принять участие";
        public bool IsRegistered { get; set; }
    }
}