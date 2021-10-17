using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using platterr_api.Entities;

namespace platterr_api.Interfaces
{
    public interface IScheduledNotificationsRepository
    {
        Task<ICollection<ScheduledNotification>> getScheduledNotifications();

        void scheduleNotification(int id, string subject, DateTime scheduledDate);

        void updateScheduledNotification(int id, string subject, DateTime scheduledDate);

        void removeScheduledNotification(int id);

        Task<bool> SaveAllAsync();
    }
}