using System.Linq;
using LokFu.Extensions;
using System.Web.Http;
using System.Runtime.Caching;
using System.Collections.Generic;

namespace LokFu.Controllers
{
    public class ClearCacheController : ApiController
    {
        public void Post()
        {
            var Form = System.Web.HttpContext.Current.Request.Form;
            string keyname = "cachename_hf";
            string cachename = Form.AllKeys.Contains(keyname) ? Form[keyname] : "";
            if (!cachename.IsNullOrEmpty())
            {
                if (cachename == "all_hf")
                {
                    List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
                    foreach (string cacheKey in cacheKeys)
                    {
                        MemoryCache.Default.Remove(cacheKey);
                    }
                    Utils.WriteLog("清空所有缓存","bug", "ClearCache");
                }
                else
                {
                    CacheBuilder.EntityCache.Remove(cachename, null);
                    Utils.WriteLog("cachename:" + cachename, "bug", "ClearCache");
                }
            }
        }

    }
}
