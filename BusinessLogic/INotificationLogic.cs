using System.Collections.Generic;
using System.Threading.Tasks;
using TauManager.Models;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface INotificationLogic
    {
        IEnumerable<NotificationViewModel> GetCurrentNotifications();
        bool ReportNotificationStatus(long id, bool success, string auditTrail);
        Task<bool> AddNotificationIfNeeded(NotificationKind kind, int relatedId);
        Task<bool> RemoveNotificationIfNeeded(NotificationKind kind, int recipientId);
    }
}