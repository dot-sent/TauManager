using System.Collections.Generic;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface INotificationLogic
    {
        IEnumerable<NotificationViewModel> GetCurrentNotifications();
        bool ReportNotificationStatus(long id, bool success, string auditTrail);
    }
}