using System;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TauManager.Utils;

namespace TauManager.BusinessLogic
{
    public class ItemLogic : IItemLogic
    {
        private TauDbContext _dbContext { get; set; }
        public ItemLogic(TauDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string[]> BulkImportFromTauHead(string urls)
        {
            List<string> status = new List<string>();
            string[] urlArray = urls.Split();
            foreach(string url in urlArray)
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    var item = await TauHead.GetItemData(url);
                    if (item == null)
                    {
                        status.Add(url + " - import error!");
                    }
                    else
                    {
                        var existingItem = _dbContext.Item.SingleOrDefault(i => i.Slug == item.Slug);
                        if (existingItem == null)
                        {
                            await _dbContext.AddAsync(item);
                            existingItem = item;
                            status.Add(url + " - import success");
                        } else {
                            status.Add(url + " - already exists");
                        }
                    }
                }
            }
            await _dbContext.SaveChangesAsync(); 
            return status.ToArray();
        }
    }
}