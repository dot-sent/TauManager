using System.Collections.Generic;
using System.Linq;
using TauManager.Models;

namespace TauManager.BusinessLogic
{
    public class IntegrationLogic : IIntegrationLogic
    {
        private TauDbContext _dbContext { get; set; }
        public IntegrationLogic(TauDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool AddDiscordOfficer(string discordLogin)
        {
            if (_dbContext.DiscordOfficer.Any(o => o.LoginName == discordLogin)) return false;
            var newOfficer = new DiscordOfficer { LoginName = discordLogin };
            _dbContext.Add(newOfficer);
            _dbContext.SaveChanges();
            return true;
        }

        public List<string> GetDiscordOfficerList()
        {
            return _dbContext.DiscordOfficer.Select(d => d.LoginName).ToList();
        }

        public bool RemoveDiscordOfficer(string discordLogin)
        {
            var officer = _dbContext.DiscordOfficer.SingleOrDefault(o => o.LoginName == discordLogin);
            if (officer == null) return false;
            _dbContext.Remove(officer);
            _dbContext.SaveChanges();
            return true;
        }
    }
}