using System;

namespace platterr_api.Entities
{
    public class ScheduledNotification
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}