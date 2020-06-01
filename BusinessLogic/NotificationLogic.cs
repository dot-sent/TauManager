using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TauManager.Models;
using TauManager.ViewModels;
using static TauManager.Models.Notification;

namespace TauManager.BusinessLogic
{
    public class NotificationLogic : INotificationLogic
    {
        private TauDbContext _dbContext { get; set; }
        public NotificationLogic(TauDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<NotificationViewModel> GetCurrentNotifications()
        {
            var notifications = _dbContext.Notification
                .Include(n => n.Recipient)
                .Where(n => n.SendAfter < DateTime.Now &&
                    n.Status == NotificationStatus.NotSent &&
                    n.Recipient.DiscordAuthConfirmed)
                .ToList();
            var notificationModels = new List<NotificationViewModel>();
            foreach (var notification in notifications)
            {
                var model = new NotificationViewModel
                {
                    Id = notification.Id,
                    RecipientDiscordLogin = notification.Recipient.DiscordLogin,
                    Message = string.Format("Hey, {0}! \n", notification.Recipient.Name),
                };

                Campaign campaign= null;

                if (notification.Kind == NotificationKind.NewCampaign || 
                    notification.Kind == NotificationKind.CampaignUpdated ||
                    notification.Kind == NotificationKind.CampaignSoon)
                {
                    campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == notification.RelatedId);
                }
                switch(notification.Kind)
                {
                    case NotificationKind.GauleVisa:
                        model.Message += "Your Gaule Visa has expired or is going to expire in 2 days and can be renewed now.";
                        break;
                    case NotificationKind.University:
                        model.Message += "Your University course is ending in 1 day - make sure to pick up a new one on time.";
                        break;
                    case NotificationKind.NewCampaign:
                        model.Message +=
                            string.Format("There is a new campaign planned in your syndicate: T{0}{1} at {2} on {3} ({4} UTC).\nMore details and signup at https://dotsent.nl/Campaigns/",
                                string.Join(null, campaign.TiersList.Select(t => t.ToString())),
                                campaign.Difficulty.ToString(),
                                campaign.Station,
                                campaign.GCTDateString,
                                campaign.UTCDateString);
                        break;
                    case NotificationKind.CampaignUpdated:
                        model.Message +=
                            string.Format("A campaign you're following has been updated: it's now T{0}{1} at {2} on {3} ({4} UTC).\nMore details at https://dotsent.nl/Campaigns/",
                                string.Join(null, campaign.TiersList.Select(t => t.ToString())),
                                campaign.Difficulty.ToString(),
                                campaign.Station,
                                campaign.GCTDateString,
                                campaign.UTCDateString);
                        break;
                    case NotificationKind.CampaignSoon:
                        model.Message +=
                            string.Format("A campaign you're following, T{0}{1} at {2}, starts in about 16 segments: on {3} ({4} UTC).\nMake sure to be there on time!",
                                string.Join(null, campaign.TiersList.Select(t => t.ToString())),
                                campaign.Difficulty.ToString(),
                                campaign.Station,
                                campaign.GCTDateString,
                                campaign.UTCDateString);
                        break;
                    case NotificationKind.NewMarketAd:
                        model.Message += "There is a new ad on the Syndicate Market, check it out at https://dotsent.nl/Market/";
                        break;
                    default:
                        break;
                }
                notificationModels.Add(model);
            }
            return notificationModels;
        }

        public bool ReportNotificationStatus(long id, bool success, string auditTrail)
        {
            var notification = _dbContext.Notification.SingleOrDefault(n => n.Id == id && n.Status == NotificationStatus.NotSent);
            if (notification == null) return false;
            notification.Audit += auditTrail;
            if (success)
            {
                notification.Status = NotificationStatus.Sent;
            } else {
                notification.RetryCount++;
                if (notification.RetryCount > Notification.MaxRetries)
                {
                    notification.Status = NotificationStatus.PermanentlyFailed;
                }
            }
            _dbContext.SaveChanges();
            return true;
        }

    }
}