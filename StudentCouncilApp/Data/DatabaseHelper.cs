using System;
using System.Linq;
using System.Collections.Generic;

namespace StudentCouncilApp.Data
{
    public class DatabaseHelper
    {
        private StudentCouncilAppEntities _context;

        public DatabaseHelper()
        {
            _context = new StudentCouncilAppEntities();
        }

        // Проверка авторизации (возвращает StudentID или 0)
        public int Authenticate(string login, string passwordHash)
        {
            var student = _context.Students.FirstOrDefault(s => s.Login == login && s.PasswordHash == passwordHash);
            return student != null ? student.StudentID : 0;
        }

        // Регистрация нового студента
        public bool Register(string login, string passwordHash, string lName, string fName,
                             string mName, string email, string phone, int groupId)
        {
            if (_context.Students.Any(s => s.Login == login))
                return false;

            var newStudent = new Student
            {
                Login = login,
                PasswordHash = passwordHash,
                LName = lName,
                FName = fName,
                MName = mName,
                Email = email,
                Phone = phone,
                GroupID = groupId,
                RoleID = 1, // 1 - студент по умолчанию
                Scores = 0
            };
            _context.Students.Add(newStudent);
            _context.SaveChanges();
            return true;
        }

        // Получить студента по ID
        public Student GetStudentById(int id)
        {
            return _context.Students.FirstOrDefault(s => s.StudentID == id);
        }

        // Получить все направления
        public List<Direction> GetDirections()
        {
            return _context.Directions.ToList();
        }

        // Получить все мероприятия (с сортировкой по дате)
        public List<Event> GetEvents()
        {
            return _context.Events.OrderBy(e => e.EventDate).ThenBy(e => e.EventTime).ToList();
        }

        // Получить ближайшее мероприятие
        public Event GetNearestEvent()
        {
            var today = DateTime.Now.Date;
            return _context.Events
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.EventTime)
                .FirstOrDefault();
        }

        // Запрос на участие
        public bool AddParticipationRequest(int studentId, int eventId, string note)
        {
            // Проверка, не подана ли уже заявка
            var existing = _context.ParticipationRequests
                .FirstOrDefault(r => r.StudentID == studentId && r.EventID == eventId);
            if (existing != null) return false;

            var request = new ParticipationRequest
            {
                StudentID = studentId,
                EventID = eventId,
                Note = note,
                RequestDate = DateTime.Now
            };
            _context.ParticipationRequests.Add(request);
            _context.SaveChanges();
            return true;
        }

        // Получить рейтинг студента (место и кол-во студентов)
        public (int place, int total) GetStudentRating(int studentId)
        {
            var student = GetStudentById(studentId);
            if (student == null) return (0, 0);

            var allStudents = _context.Students.OrderByDescending(s => s.Scores).ToList();
            int place = allStudents.FindIndex(s => s.StudentID == studentId) + 1;
            return (place, allStudents.Count);
        }

        // Получить достижения студента (документы)
        public List<Document> GetStudentDocuments(int studentId)
        {
            return _context.Documents.Where(d => d.StudentID == studentId).OrderByDescending(d => d.UploadDate).ToList();
        }

        // Добавить достижение
        public void AddDocument(int studentId, string link, string note)
        {
            var doc = new Document
            {
                StudentID = studentId,
                DocumentLink = link,
                Note = note,
                UploadDate = DateTime.Now
            };
            _context.Documents.Add(doc);
            _context.SaveChanges();
        }

        // Обновить счетчик баллов студента
        public void UpdateStudentScores(int studentId, int additionalScores)
        {
            var student = GetStudentById(studentId);
            if (student != null)
            {
                student.Scores += additionalScores;
                _context.SaveChanges();
            }
        }

        // Получить все группы
        public List<Group> GetAllGroups()
        {
            return _context.Groups.ToList();
        }

        // Получить случайную цитату (пока из массива, потом можно в БД)
        public string GetRandomQuote()
        {
            string[] quotes = {
        "Сегодня - ты просто студент, а завтра - легенда колледжа. Запишись на событие!",
        "Успех приходит к тем, кто действует! Участвуй в мероприятиях!",
        "Каждое событие - это шаг к твоей мечте. Не пропускай!",
        "Твой рейтинг растет с каждым участием. Вперед!",
        "Лучший студент - не тот, кто учится на 5, а тот кто успевает везде!"
    };

            Random rnd = new Random();
            return quotes[rnd.Next(quotes.Length)];
        }

        // Получить последние 3 фото мероприятий для ленты достижений
        public List<EventPhoto> GetRecentEventPhotos()
        {
            return _context.EventPhotos
                .OrderByDescending(p => p.UploadDate)
                .Take(3)
                .ToList();
        }

        // Получить количество участников мероприятия
        public int GetEventParticipantsCount(int eventId)
        {
            return _context.StudentParticipations
                .Count(p => p.EventID == eventId && p.Confirmation == true);
        }

        // Проверить, записан ли студент на мероприятие
        public bool IsStudentRegisteredToEvent(int studentId, int eventId)
        {
            return _context.ParticipationRequests
                .Any(r => r.StudentID == studentId && r.EventID == eventId);
        }

        // Получить руководителя направления
        public Student GetDirectionSupervisor(int directionId)
        {
            var supervisor = _context.Supervisors
                .FirstOrDefault(s => s.DirectionID == directionId);

            if (supervisor != null)
                return _context.Students.FirstOrDefault(s => s.StudentID == supervisor.StudentID);

            return null;
        }

        // Получить телефон руководителя направления
        public string GetSupervisorPhone(int directionId)
        {
            var supervisor = GetDirectionSupervisor(directionId);
            return supervisor?.Phone ?? "Нет данных";
        }

        // Получить Telegram ссылку направления (пока заглушка, потом можно добавить в БД)
        public string GetDirectionTelegramLink(int directionId)
        {
            // Временные ссылки для каждого направления
            var telegramLinks = new Dictionary<int, string>
    {
        { 1, "https://t.me/volunteer_channel" },
        { 2, "https://t.me/sport_channel" },
        { 3, "https://t.me/science_channel" },
        { 4, "https://t.me/cybersport_channel" },
        { 5, "https://t.me/media_channel" },
        { 6, "https://t.me/culture_channel" },
        { 7, "https://t.me/teamlead_channel" }
    };

            return telegramLinks.ContainsKey(directionId) ? telegramLinks[directionId] : "https://t.me/student_council";
        }

        // Получить приветственное сообщение руководителя (пока заглушка)
        public string GetSupervisorWelcomeMessage(int directionId, string supervisorName)
        {
            var messages = new Dictionary<int, string>
    {
        { 1, $"Привет! Я {supervisorName}, руководитель волонтерского направления. Мы занимаемся экологией, помощью животным и добрыми делами. Присоединяйся к нашей команде!" },
        { 2, $"Привет! Я {supervisorName}, руководитель спортивного клуба. Турники, мячи, рекорды и здоровый образ жизни - это к нам!" },
        { 3, $"Привет! Я {supervisorName}, руководитель научного общества. Лабы, конференции, интеллектуальные игры - развиваем науку вместе!" },
        { 4, $"Привет! Я {supervisorName}, руководитель киберспортивной лиги. Dota, CS, Valorant и другие игры - покажи свой скилл!" },
        { 5, $"Привет! Я {supervisorName}, руководитель медиа-центра. Снимаем TikTok, пишем посты, верстаем газеты - прокачай личный бренд!" },
        { 6, $"Привет! Я {supervisorName}, руководитель культмасса. Вечеринки, КВН, творчество - создаем настроение!" },
        { 7, $"Привет! Я {supervisorName}, руководитель направления тим-лидеров. Командообразование, тренинги, лидерство - растим капитанов!" }
    };

            return messages.ContainsKey(directionId) ? messages[directionId] : $"Привет! Я {supervisorName}, присоединяйся к нашему направлению!";
        }

        // Получить мероприятия с фильтром по направлению
        public List<Event> GetEventsByDirection(int? directionId = null)
        {
            var query = _context.Events.AsQueryable();

            if (directionId.HasValue && directionId.Value > 0)
                query = query.Where(e => e.DirectionID == directionId.Value);

            return query.OrderBy(e => e.EventDate)
                        .ThenBy(e => e.EventTime)
                        .ToList();
        }

        // Получить все заявки студента
        public List<ParticipationRequest> GetStudentRequests(int studentId)
        {
            return _context.ParticipationRequests
                .Where(r => r.StudentID == studentId)
                .ToList();
        }

        // Получить подтвержденные участия студента
        public List<StudentParticipation> GetStudentParticipations(int studentId)
        {
            return _context.StudentParticipations
                .Where(p => p.StudentID == studentId && p.Confirmation == true)
                .ToList();
        }

        // Получить название направления по ID
        public string GetDirectionName(int directionId)
        {
            var direction = _context.Directions.FirstOrDefault(d => d.DirectionID == directionId);
            return direction?.Name ?? "Неизвестно";
        }

        // Получить список участников мероприятия (подтвержденных)
        public List<Student> GetEventParticipants(int eventId)
        {
            var participants = from p in _context.StudentParticipations
                               join s in _context.Students on p.StudentID equals s.StudentID
                               where p.EventID == eventId && p.Confirmation == true
                               select s;
            return participants.ToList();
        }

        // Получить статус заявки студента на мероприятие
        public string GetRequestStatus(int studentId, int eventId)
        {
            var request = _context.ParticipationRequests
                .FirstOrDefault(r => r.StudentID == studentId && r.EventID == eventId);

            if (request != null)
                return "Заявка отправлена";

            var participation = _context.StudentParticipations
                .FirstOrDefault(p => p.StudentID == studentId && p.EventID == eventId && p.Confirmation == true);

            if (participation != null)
                return "✓ Участвую";

            return "Можно записаться";
        }

        // Получить группу студента
        public Group GetStudentGroup(int groupId)
        {
            return _context.Groups.FirstOrDefault(g => g.GroupID == groupId);
        }

        // Обновить данные студента
        public bool UpdateStudentProfile(int studentId, string email, string phone, string login)
        {
            try
            {
                var student = _context.Students.FirstOrDefault(s => s.StudentID == studentId);
                if (student != null)
                {
                    student.Email = email;
                    student.Phone = phone;
                    student.Login = login;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Получить все достижения студента с информацией о мероприятии
        public List<Document> GetStudentAchievements(int studentId)
        {
            return _context.Documents
                .Where(d => d.StudentID == studentId)
                .OrderByDescending(d => d.UploadDate)
                .ToList();
        }

        // Добавить достижение с фото (сохраняем путь к файлу)
        public bool AddAchievement(int studentId, string documentLink, string note)
        {
            try
            {
                var doc = new Document
                {
                    StudentID = studentId,
                    DocumentLink = documentLink,
                    Note = note,
                    UploadDate = DateTime.Now
                };
                _context.Documents.Add(doc);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Удалить достижение
        public bool DeleteAchievement(int documentId)
        {
            try
            {
                var doc = _context.Documents.FirstOrDefault(d => d.DocumentID == documentId);
                if (doc != null)
                {
                    _context.Documents.Remove(doc);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Получить мероприятия, на которые записан студент (с подтверждением)
        public List<Event> GetStudentRegisteredEvents(int studentId)
        {
            var eventIds = _context.StudentParticipations
                .Where(p => p.StudentID == studentId && p.Confirmation == true)
                .Select(p => p.EventID)
                .ToList();

            return _context.Events.Where(e => eventIds.Contains(e.EventID)).ToList();
        }

        // Получить заявки студента на мероприятия (ожидающие)
        public List<Event> GetStudentPendingEvents(int studentId)
        {
            var eventIds = _context.ParticipationRequests
                .Where(r => r.StudentID == studentId)
                .Select(r => r.EventID)
                .ToList();

            return _context.Events.Where(e => eventIds.Contains(e.EventID)).ToList();
        }
    }
}