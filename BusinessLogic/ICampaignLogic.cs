using System.Security.Claims;
using System.Threading.Tasks;
using TauManager.Models;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface ICampaignLogic
    {
        CampaignOverviewViewModel GetCampaignOverview(int playerId, bool showLootApplyButton, bool showLootEditControls, bool showAwardButton, int syndicateId);
        CampaignDetailsViewModel GetCampaignById(int id, bool showLootApplyButton, bool showLootEditControls, int syndicateId);
        CampaignDetailsViewModel GetNewCampaign(int syndicateId);
        Task<Campaign> CreateOrEditCampaign(Campaign campaign, int syndicateId);
        Task<LootItemViewModel> AddLootByTauheadURL(int campaignId, string url, bool showLootApplyButton, bool showLootEditControls);
        Task<CampaignPageParseResultViewModel> ParseCampaignPage(string fileContents, int campaignId);
        Task<bool> SetSignupStatus(int playerId, int campaignId, bool status);
        // Task<bool> ParseAttendanceCSV(string fileContents);
        AttendanceViewModel GetCampaignAttendance(int? playerId, int syndicateId);
        bool PlayerCanEditCampaign(int? playerId, int campaignId);
        bool PlayerCanVolunteerForCampaign(int? playerId, int campaignId);
        Task<bool> VolunteerForCampaign(int? playerId, int campaignId);
        LeaderboardViewModel GetLeaderboard(int? playerId);
    }
}