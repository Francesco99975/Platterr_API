using System;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using FirebaseAdmin.Messaging;
using platterr_api.Interfaces;

namespace platterr_api.Helpers
{
    public class NotificationsSender : IInvocable
    {
        private readonly IScheduledNotificationsRepository _scheduledNotificationsRepository;
        public NotificationsSender(IScheduledNotificationsRepository scheduledNotificationsRepository)
        {
            _scheduledNotificationsRepository = scheduledNotificationsRepository;
        }

        public async Task Invoke()
        {
            var now = DateTime.Now;
            var scheduledNotifications = await _scheduledNotificationsRepository.getScheduledNotifications();

            var sendingNotifications = scheduledNotifications.Where(x => x.ScheduledDate.Year == now.Year && x.ScheduledDate.Month == now.Month && x.ScheduledDate.Day == now.Day).ToList();

            if (sendingNotifications.Count > 0)
            {
                foreach (var notif in sendingNotifications)
                {
                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = "Order for: " + notif.Subject,
                            Body = "This order is due tomorrow"
                        },
                        Android = new FirebaseAdmin.Messaging.AndroidConfig()
                        {
                            Priority = Priority.Normal,
                            TimeToLive = TimeSpan.FromHours(1),
                            RestrictedPackageName = "dev.francescobarranca.platterr",
                        },
                        Topic = "all"
                    };

                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    _scheduledNotificationsRepository.removeScheduledNotification(notif.Id);
                    await _scheduledNotificationsRepository.SaveAllAsync();
                }
            }
        }
    }
}