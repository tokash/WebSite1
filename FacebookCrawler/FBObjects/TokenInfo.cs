using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookCrawler.FBObjects
{
    [Serializable]
    public class TokenInfo
    {
        public TokenInfo()
        {
            data = new Data();
        }

        public Data data { get; set; }
    }

    [Serializable]
    public class Data
    {
        public Data()
        {
            scopes = new List<string>();
        }

        public string app_id { get; set; }
        public string application { get; set; }
        public int expires_at { get; set; }
        public bool is_valid { get; set; }
        public int issued_at { get; set; }
        public List<string> scopes { get; set; }
        public int user_id { get; set; }
    }
}
