namespace StudentCouncilApp.Models
{
    public class DirectionModel
    {
        public int DirectionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorPhoto { get; set; } // Пока заглушка
        public string WelcomeMessage { get; set; }
        public string Phone { get; set; }
        public string TelegramLink { get; set; }

        // Цвета для карточек
        public string CardColor { get; set; }
    }
}