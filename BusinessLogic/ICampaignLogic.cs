using System.Security.Claims;
using System.Threading.Tasks;
using TauManager.Models;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface ICampaignLogic
    {
        CampaignOverviewViewModel GetCampaignOverview(int playerId, bool showLootApplyButton, bool showLootEditControls);
        CampaignDetailsViewModel GetCampaignById(int id, bool showLootApplyButton, bool showLootEditControls);
        CampaignDetailsViewModel GetNewCampaign();
        Task<Campaign> CreateOrEditCampaign(Campaign campaign);
        Task<LootItemViewModel> AddLootByTauheadURL(int campaignId, string url, bool showLootApplyButton, bool showLootEditControls);
        Task<CampaignPageParseResultViewModel> ParseCampaignPage(string fileContents, int campaignId);
        Task<bool> SetSignupStatus(int playerId, int campaignId, bool status);
        Task<bool> ParseAttendanceCSV(string fileContents);
        AttendanceViewModel GetCampaignAttendance(int? playerId);
    }
}