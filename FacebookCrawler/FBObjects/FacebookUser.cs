using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookCrawler.FBObjects
{
    class FacebookUser
    {
        public string id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public Uri PictureUri { get; set; }
    }
}
