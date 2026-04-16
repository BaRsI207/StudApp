using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using StudentCouncilApp.Data;

namespace StudentCouncilApp
{
    public partial class SupervisorReportsTab : UserControl
    {
        private DatabaseHelper _db;
        private List<Direction> _directions;

        public SupervisorReportsTab(DatabaseHelper db, List<Direction> directions)
        {
            InitializeComponent();
            _db = db;
            _directions = directions;

            LoadStatistics();
        }

        private void LoadStatistics()
        {
            int totalEvents = 0;
            int totalParticipants = 0;
            int totalRequests = 0;

            foreach (var direction in _directions)
            {
                var events = _db.GetEventsByDirection(direction.DirectionID);
                totalEvents += events.Count;

                foreach (var ev in events)
                {
                    totalParticipants += _db.GetEventParticipantsCount(ev.EventID);
                    totalRequests += _db.GetRequestsByEvent(ev.EventID).Count;
                }
            }

            txtStats.Text = $"📅 Всего мероприятий: {totalEvents}\n" +
                           $"👥 Всего участников: {totalParticipants}\n" +
                           $"📝 Всего заявок: {totalRequests}\n" +
                           $"🎯 Направлений под управлением: {_directions.Count}";
        }
    }
}