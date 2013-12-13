using System;
using System.Web.Script.Serialization;
using Facebook;
using FacebookCrawler.FBObjects;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;

namespace FacebookCrawler
{
    class FaceBookAPI
    {
        private static string AppID = "630904423618987";
        private static string AppSecret = "485456384f450bde62bc9eacbfc2c316";
        private static string AppToken = "630904423618987|NTKVsaFHzzi4E-sD2Rodb8ECSCE";

        /*TODO:
         * Add function IsTokenValid
         */
        public FaceBookAPI()
        {
            _FBClient = new FacebookClient();
            _JsonSerializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            _ConfigurationValues = (NameValueCollection)ConfigurationManager.GetSection("Data");

            GetUserAccessToken();
        }

        public FaceBookAPI(string iAccessToken)
        {
            _FBClient = new FacebookClient(iAccessToken);
            _JsonSerializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            _ConfigurationValues = (NameValueCollection)ConfigurationManager.GetSection("Data");

            GetUserAccessToken();
        }


        #region Memebers
        FacebookClient _FBClient;
        JavaScriptSerializer _JsonSerializer = new JavaScriptSerializer();
        string _UserAccessToken;
        string _ApplicationAccessToken;
        NameValueCollection _ConfigurationValues;

        public string AccessToken { get { return _FBClient.AccessToken; } } 
        #endregion

        public bool IsTokenValid(string iAccessToken, ref TokenInfo oTokenInfo)
        {
            
            bool isValid = false;

            try
            {
                object result = _FBClient.Get("debug_token", new
                    {
                        input_token =_FBClient.AccessToken,
                        access_token = AppToken
                    });

                TokenInfo tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(result.ToString());
                oTokenInfo = tokenInfo;

                if (tokenInfo.data.is_valid)
                {
                    isValid = true;
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }

            


            return isValid;
        }

        public void GetNewShortLivedUserAccessToken()
        {
            try
            {
                dynamic result = _FBClient.Get("oauth/access_token", new
                    {
                        client_id = AppID,
                        client_secret = AppSecret,
                        grant_type = "client_credentials"
                    });

                _UserAccessToken = result.access_token;
                _FBClient.AccessToken = _UserAccessToken;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void GetApplicationAccessToken()
        {

        }

        private void GetUserAccessToken()
        {
            String line;

            try
            {
                if (_ConfigurationValues != null)
                {
                    FaceBookToken facebookToken = new FaceBookToken();
                    string accessTokenFilename = _ConfigurationValues["AccessTokenFileName"];
                    string path = Path.GetTempPath() + accessTokenFilename;

                    using (StreamReader sr = new StreamReader(path))
                    {
                        line = sr.ReadToEnd();
                    }

                    string[] tokenFileSplit = line.Split('\n');
                    facebookToken.access_token = tokenFileSplit[0];
                    facebookToken.expires = DateTime.Parse(tokenFileSplit[1]);

                    if (DateTime.Now < facebookToken.expires)
                    {
                        _FBClient.AccessToken = facebookToken.access_token;
                    }
                    else
                    {
                        //Need to get a new access token, in the meanwhile throw exception
                        throw new Exception("Current user access token has expired");
                    }
                }
                else
                {
                    throw new Exception("No user token source, user token is needed before any contact is made with facebook");
                }
            }
            catch (Exception)
            {
                throw;
            }

            

        }

        public void PrintTokenInfo(TokenInfo iTokenInfo)
        {
            if (iTokenInfo != null)
            {
                if (iTokenInfo.data != null)
                {
                    Console.WriteLine("Token Information");
                    Console.WriteLine("-----------------");
                    if (iTokenInfo.data.app_id != null)
                    {
                        Console.WriteLine(string.Format("Application ID: {0}", iTokenInfo.data.app_id)); 
                    }

                    if (iTokenInfo.data.user_id != null && iTokenInfo.data.user_id != 0)
                    {
                        Console.WriteLine(string.Format("User ID: {0}", iTokenInfo.data.user_id));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("User ID: {0}", "Unknown User ID"));
                    }

                    if (iTokenInfo.data.application != null)
                    {
                        Console.WriteLine(string.Format("Application: {0}", iTokenInfo.data.application));
                    }

                    if (iTokenInfo.data.issued_at != null && iTokenInfo.data.issued_at != 0)
                    {
                        Console.WriteLine(string.Format("Issued at: {0}", Facebook.DateTimeConvertor.FromUnixTime( iTokenInfo.data.issued_at)));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Issued at: {0}", "Unknown time"));
                    }

                    if (iTokenInfo.data.expires_at != null && iTokenInfo.data.expires_at != 0)
                    {
                        Console.WriteLine(string.Format("Expires at: {0}", iTokenInfo.data.expires_at));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Expires at: {0}", "Unknown time"));
                    }

                    if (iTokenInfo.data.is_valid != null)
                    {
                        Console.WriteLine(string.Format("Is valid: {0}", iTokenInfo.data.is_valid.ToString()));
                    }

                    if (iTokenInfo.data.scopes != null && iTokenInfo.data.scopes.Count != 0)
                    {
                        Console.WriteLine("Scopes:");
                        foreach (string scope in iTokenInfo.data.scopes)
                        {
                            Console.WriteLine(string.Format("{0}", scope));
                        }
                    }
                }
            }
        }

        public void GetUserComments(string iUserName)
        {
            //GET /me/feed
            try
            {

                dynamic result = _FBClient.Get(string.Format("/{0}/feed", iUserName));

                var info = _JsonSerializer.DeserializeObject(result.ToString());

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
