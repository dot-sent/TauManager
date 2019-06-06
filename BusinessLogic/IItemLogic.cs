using System.Threading.Tasks;

namespace TauManager.BusinessLogic
{
    public interface IItemLogic
    {
        Task<string[]> BulkImportFromTauHead(string urls);
    }
}