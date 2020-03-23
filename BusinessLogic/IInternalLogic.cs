using System.Threading.Tasks;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface IInternalLogic
    {
        Task<ActionResponseViewModel> ImportItemsFromTauhead(string jsonContent);
    }
}