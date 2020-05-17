using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TauManager.Models;
using TauManager.ViewModels;
using static TauManager.Models.Notification;
using static TauManager.Models.Player;

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
            return _dbContext.Notification
                .Include(n => n.Recipient)
                .Where(n => n.SendAfter < DateTime.Now && n.Status == NotificationStatus.NotSent)
                .Select(n => new NotificationViewModel{
                    Id = n.Id,
                    Message = ComposeNotificationMessage(n),
                    RecipientDiscordLogin = n.Recipient.DiscordLogin
                });
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

        private static string ComposeNotificationMessage(Notification notification)
        {
            string message = "";
            switch (notification.Kind)
            {
                case NotificationKind.GauleVisa:
                    message = "Your Gaule Visa is going to expire in 2 days and can be renewed now.";
                    break;
                case NotificationKind.University:
                    message = "Your University course is ending in 1 day - make sure to pick up a new one on time.";
                    break;
                case NotificationKind.NewCampaign:
                    message = "There is a new campaign planned in your syndicate - check it out at https://dotsent.nl/Campaigns/";
                    break;
                case NotificationKind.CampaignUpdated:
                    message = "A campaign you're following has been updated - check it out at https://dotsent.nl/Campaigns/";
                    break;
                case NotificationKind.CampaignSoon:
                    message = "A campaign you're following is starting in about 16 segments - make sure to be there on time!";
                    break;
                case NotificationKind.NewMarketAd:
                    message = "There is a new ad on the Syndicate Market, check it out at https://dotsent.nl/Market/";
                    break;
                default:
                    break;
            }
            return message;
        }

        public async Task<bool> AddNotificationIfNeeded(NotificationKind kind, int relatedId)
        {
            switch (kind)
            {
                case NotificationKind.GauleVisa:
                    {
                        var player = _dbContext.Player.FirstOrDefault(p => p.Id == relatedId);
                        _dbContext.Notification.Add(
                            new Notification{
                                Kind = kind,
                                RecipientId = relatedId,
                                SendAfter = player.GauleVisaExpiry.Value.AddDays(-2),
                                Status = NotificationStatus.NotSent
                            }
                        );
                    }
                    break;
                case NotificationKind.University:
                    {
                        var player = _dbContext.Player.FirstOrDefault(p => p.Id == relatedId);
                        _dbContext.Notification.Add(
                            new Notification{
                                Kind = kind,
                                RecipientId = relatedId,
                                SendAfter = player.UniCourseDate.Value.AddDays(-1),
                                Status = NotificationStatus.NotSent
                            }
                        );
                    }
                    break;
                case NotificationKind.NewCampaign:
                    {
                        var campaign = _dbContext.Campaign
                            .Include(c => c.Signups)
                            .FirstOrDefault(c => c.Id == relatedId);
                        var playersToNotify = _dbContext.Player
                            .Where(p =>
                                p.SyndicateId == campaign.SyndicateId &&
                                (p.NotificationSettings & NotificationFlags.NewCampaign) == NotificationFlags.NewCampaign
                            );
                        foreach (var player in playersToNotify)
                        {
                            _dbContext.Add(
                                new Notification{
                                    Kind = kind,
                                    RecipientId = player.Id,
                                    SendAfter = DateTime.Now,
                                    Status = NotificationStatus.NotSent
                                }
                            );
                        }
                        var playersToNotifyBeforeCamp = _dbContext.Player
                            .Where(p =>
                                p.SyndicateId == campaign.SyndicateId &&
                                (p.NotificationSettings & NotificationFlags.CampaignSoonAll) == NotificationFlags.CampaignSoonAll
                            );
                        foreach (var player in playersToNotifyBeforeCamp)
                        {
                            _dbContext.Add(
                                new Notification{
                                    Kind = NotificationKind.CampaignSoon,
                                    RecipientId = player.Id,
                                    SendAfter = campaign.UTCDateTime.Value.AddHours(-4),
                                    Status = NotificationStatus.NotSent
                                }
                            );
                        }
                    }
                    break;
                case NotificationKind.CampaignUpdated:
                    break;
                case NotificationKind.CampaignSoon:
                    break;
                case NotificationKind.NewMarketAd:
                    break;
                default:
                    break;
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveNotificationIfNeeded(NotificationKind kind, int recipientId)
        {
            switch (kind)
            {
                case NotificationKind.GauleVisa:
                case NotificationKind.University:
                    {
                        var notifications =_dbContext.Notification
                            .Where(n => n.RecipientId == recipientId &&
                                n.Kind == kind);
                        _dbContext.RemoveRange(notifications);
                    }
                    break;
                case NotificationKind.CampaignSoon:
                    {
                        var notifications =_dbContext.Notification
                            .Where(n => n.RecipientId == recipientId &&
                                n.Kind == NotificationKind.CampaignSoon);
                        _dbContext.RemoveRange(notifications);
                    }
                    break;
                default:
                    break;
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}