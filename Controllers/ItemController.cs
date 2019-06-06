using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Utils;

namespace TauManager.Controllers
{
    [AuthorizeRoles(ApplicationRoleManager.Administrator)]
    public class ItemController: Controller
    {
        public IItemLogic _itemLogic { get; set; }
        public ItemController(IItemLogic logic)
        {
            _itemLogic = logic;
        }
        public IActionResult BulkImportFromTauHead()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkImportFromTauHead(string urls)
        {
            var model = await _itemLogic.BulkImportFromTauHead(urls);
            return View(model);
        }
    }
}