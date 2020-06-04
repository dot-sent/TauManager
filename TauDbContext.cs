using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TauManager.Models;

namespace TauManager
{
    public class TauDbContext : DbContext
    {
        public TauDbContext(DbContextOptions<TauDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.UseSerialColumns();


        #region Player-related data
        public DbSet<Player> Player { get; set; }
        public DbSet<PlayerHistory> PlayerHistory { get; set; }
        public DbSet<PlayerSkill> PlayerSkill { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<SkillGroup> SkillGroup { get; set; }
        #endregion
        #region Generic data
        public DbSet<Item> Item { get; set; }
        #endregion
        #region Campaign-related data
        public DbSet<Campaign> Campaign { get; set; }
        public DbSet<CampaignAttendance> CampaignAttendance { get; set; }
        public DbSet<CampaignLoot> CampaignLoot { get; set; }
        public DbSet<CampaignSignup> CampaignSignup { get; set; }
        public DbSet<LootRequest> LootRequest { get; set; }
        public DbSet<PlayerListPositionHistory> PlayerListPositionHistory { get; set; }
        #endregion
        #region Syndicate-related data
        public DbSet<Syndicate> Syndicate { get; set; }
        public DbSet<SyndicateHistory> SyndicateHistory { get; set; }
        #endregion
        #region Syndicate Market
        public DbSet<MarketAd> MarketAd { get; set; }
        public DbSet<MarketAdBundle> MarketAdBundle { get; set; }
        public DbSet<MarketAdBundleItem> MarketAdBundleItem { get; set; }
        public DbSet<MarketAdReaction> MarketAdReaction { get; set; }
        #endregion
        #region Notifications
        public DbSet<Notification> Notification { get; set; }
        #endregion
        #region Integrations
        public DbSet<DiscordOfficer> DiscordOfficer { get; set; }
        #endregion
    }
}
