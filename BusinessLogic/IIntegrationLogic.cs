using System.Collections.Generic;

namespace TauManager.BusinessLogic
{
    public interface IIntegrationLogic
    {
        List<string> GetDiscordOfficerList();
        bool AddDiscordOfficer(string discordLogin);
        bool RemoveDiscordOfficer(string discordLogin);
    }
}