using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class RatingPage : UserControl
    {
        public RatingPage(int studentId, DatabaseHelper db)
        {
            InitializeComponent();

            var student = db.GetStudentById(studentId);
            var rating = db.GetStudentRating(studentId);

            txtRatingInfo.Text = $"🏆 Рейтинг студентов 🏆\n\n" +
                                $"{student.FName} {student.LName}\n\n" +
                                $"⭐ Баллов: {student.Scores}\n" +
                                $"📊 Место: {rating.place} из {rating.total}\n\n" +
                                $"Продолжай участвовать в мероприятиях,\n" +
                                $"чтобы поднять свой рейтинг! 🦝";
        }
    }
}