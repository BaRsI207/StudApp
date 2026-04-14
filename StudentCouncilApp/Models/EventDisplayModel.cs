using System;

namespace StudentCouncilApp.Models
{
    public class EventDisplayModel
    {
        public int EventID { get; set; }
        public string Name { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Place { get; set; }
        public int Scores { get; set; }
        public int ExpectedStudentsAmount { get; set; }
        public int ActualStudentsAmount { get; set; }
        public string DirectionName { get; set; }
        public int DirectionID { get; set; }
        public string Status { get; set; }
        public int ParticipantsCount { get; set; }

        // Форматированная дата
        public string FormattedDate
        {
            get
            {
                var today = DateTime.Now.Date;
                var eventDate = EventDate.Date;

                if (eventDate == today)
                    return $"⏰ Сегодня в {EventTime:hh\\:mm}";
                else if (eventDate == today.AddDays(1))
                    return $"⏰ Завтра в {EventTime:hh\\:mm}";
                else if (eventDate < today)
                    return $"📅 Прошло {EventDate:dd.MM.yyyy}";
                else
                    return $"📅 {EventDate:dd.MM.yyyy} в {EventTime:hh\\:mm}";
            }
        }

        // Цвет статуса
        public string StatusColor
        {
            get
            {
                if (Status == "✓ Участвую")
                    return "#27AE60";
                if (Status == "Заявка отправлена")
                    return "#F39C12";
                return "#3498DB";
            }
        }

        // Цвет карточки
        public string CardBorderColor
        {
            get
            {
                if (EventDate.Date < DateTime.Now.Date)
                    return "#BDC3C7";
                return "#3498DB";
            }
        }

        // Активно ли мероприятие (не прошло)
        public bool IsActive => EventDate.Date >= DateTime.Now.Date;
    }
}