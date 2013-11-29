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
        private static string UserToken = "CAAI9zeIOmasBAF7qLfY8WIXbs5RTz1DZBI7N2YJtirZCZB1FeHRJZCsNFkhZCFJyhVdcVKqNGdg7sRs4mQfis5mRmQ4eMZCGZBEMoMyD44WvRRAmXLWgScTJjTGRsxVPUHjkUswMbMX3ZAbOA6CoyNRdsPdETe4ZCrOFjET3uNHUUafR4EgnguVstlpL19ZCNKCU706cvHZBM8pjnqxmADoqitYOnuoZC4TyEtUGnKpxzbG7zwZDZD";
        

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
            TokenInfo tokenInfo = null;

            try
            {
                fb.GetNewShortLivedAccessToken();
                if (fb.IsTokenValid(fb.AccessToken, ref tokenInfo))
                {
                    fb.GetUserComments("keshet.mako");
                }

                

                //if (tokenInfo != null)
                //{
                //    fb.PrintTokenInfo(tokenInfo);
                //}
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

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
