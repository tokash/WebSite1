using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facebook;
using FacebookCrawler.FBObjects;

namespace FacebookCrawler
{
    class Program
    {
        private static string AppID = "630904423618987";
        private static string AppSecret = "485456384f450bde62bc9eacbfc2c316";
        private static string ClientToken = "35673ccf126aaf11244cbd1b9b813c84";
        private static string AppToken = "630904423618987|NTKVsaFHzzi4E-sD2Rodb8ECSCE";
        private static string UserToken = "CAAI9zeIOmasBAA7uzNklhZC8xSGGTZBHUjJZBYkLDgBtn2AXLxDaZCSs0NshY68b7OLuVZAUZCUy71gMIg29jlfqdZBb3wO9kRBFF1td77hxyqkOXMmxKTPh65RtfkCNlDoxx8cPzhx08zxsrcRIkZBjbDJj341kTJOPk88baTjvZBk7MSZCwH9w6jcKW3dAm2ZA3kOCQzlEGvNcFpijYsJiwD6I0K1M9m7SzTClvmZAsfzmnQZDZD";
        

        static void Main(string[] args)
        {

            //FacebookClient fb = new FacebookClient();
            //dynamic result = fb.Get("oauth/access_token", new
            //{
            //    client_id = AppID,
            //    client_secret = AppSecret,
            //    grant_type = "client_credentials"
            //});

            FaceBookAPI fb = new FaceBookAPI();

            //"[א-ת]\'"
            List<Datum> posts = fb.GetPostsMatchingRegexPattern("tzahalonline", "[\u0591-\u05F4][\u0591-\u05F4]\"[\u0591-\u05F4] [\u0591-\u05F4]\'", new DateTime(2011, 1, 1), DateTime.Now);//fb.GetAllPostsFromFeed("yossi.tokash");

            //    //if (tokenInfo != null)
            //    //{
            //    //    fb.PrintTokenInfo(tokenInfo);
            //    //}
            //}
            //catch (Exception ex)
            //{

            //    Console.WriteLine(ex.ToString());
            //}

            //fb.AccessToken = result.access_token;

            //try
            //{
            //    dynamic me = fb.Get("keshet.mako");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }
    }
}
