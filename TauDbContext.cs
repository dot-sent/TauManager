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

        #region Player-related data
        public DbSet<Player> Player { get; set; }
        public DbSet<PlayerHistory> PlayerHistory { get; set; }
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
    }
}
