using TauManager.Models;

namespace TauManager.ViewModels
{
    public class NotificationViewModel
    {
        public long Id { get; set; }
        public string RecipientDiscordLogin { get; set; }
        public string Message { get; set; }
    }
}