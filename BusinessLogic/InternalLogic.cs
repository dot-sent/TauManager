using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class InternalLogic : IInternalLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ITauHeadClient _tauHead { get; set; }

        public InternalLogic(TauDbContext dbContext, ITauHeadClient tauHead)
        {
            _dbContext = dbContext;
            _tauHead = tauHead;
        }

        public async Task<ActionResponseViewModel> ImportItemsFromTauhead(string jsonContent)
        {
            var result = _tauHead.BulkParseItems(jsonContent);
            var response = new ActionResponseViewModel();
            var itemIds = new Dictionary<int, bool>();
            foreach(var slug in result.Keys)
            {
                if (result[slug] == null)
                {
                    response.Message += string.Format("Error parsing item with slug {0}\n", slug);
                } else {
                    var itemExists = _dbContext.Item.Any(i => i.Slug == slug);
                    if (itemExists)
                    {
                        var thItem = result[slug];
                        var existingItem = _dbContext.Item.AsNoTracking().SingleOrDefault(i => i.Slug == slug);
                        thItem.Id = existingItem == null ? 0 : existingItem.Id;
                        if (itemIds.ContainsKey(thItem.Id))
                        {
                            response.Message += string.Format("Can't update item with slug {0} - item already exists and conficting id {1} is already present.\n", slug, thItem.Id);
                        } else if(thItem.Id > 0) {
                            itemIds[thItem.Id] = true;
                            _dbContext.Item.Update(thItem);
                            response.Message += string.Format("Updating item with slug {0} since item already exists\n", slug);
                        } else {
                            response.Message += string.Format("Something weird happened with item with slug {0} - please investigate\n", slug);
                        }
                    } else {
                        _dbContext.Item.Add(result[slug]);
                        response.Message += string.Format("Item with slug {0} imported OK\n", slug);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return response;
        }
    }
}