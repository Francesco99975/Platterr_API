using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using platterr_api.Entities;
using platterr_api.Interfaces;

namespace platterr_api.Data
{
    public class ScheduledNotificationsRepository : IScheduledNotificationsRepository
    {
        private readonly DataContext _context;
        public ScheduledNotificationsRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ICollection<ScheduledNotification>> getScheduledNotifications()
        {
            return await _context.ScheduledNotifications.ToListAsync();
        }

        public void removeScheduledNotification(int id)
        {
            _context.ScheduledNotifications.Remove(_context.ScheduledNotifications.Where(x => x.Id == id).FirstOrDefault());
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void scheduleNotification(int id, string subject, DateTime scheduledDate)
        {
            var newScheduledNotification = new ScheduledNotification
            {
                Id = id,
                Subject = subject,
                ScheduledDate = scheduledDate
            };

            _context.ScheduledNotifications.Add(newScheduledNotification);
        }

        public void updateScheduledNotification(int id, string subject, DateTime scheduledDate)
        {
            var updatedScheduledNotification = _context.ScheduledNotifications.Where(x => x.Id == id).FirstOrDefault();

            updatedScheduledNotification.Subject = subject;
            updatedScheduledNotification.ScheduledDate = scheduledDate;

            _context.ScheduledNotifications.Update(updatedScheduledNotification);
        }
    }
}