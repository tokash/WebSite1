using System;
using System.Web.Script.Serialization;
using Facebook;
using FacebookCrawler.FBObjects;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iUserName"></param>
        /// <param name="iSince"></param>
        /// <param name="iUntil"></param>
        /// <returns></returns>
        public List<Datum> GetPostsFromFeed(string iUserName, DateTime iSince, DateTime iUntil)
        {
            List<Datum> posts = new List<Datum>();

            //GET /me/feed
            while (true)
            {
                try
                {
                    object result = _FBClient.Get(string.Format("/{0}/feed", iUserName));
                    string res = result.ToString();

                    FacebookFeed facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);
                    //posts.AddRange(facebookFeed.data);

                    foreach (Datum post in facebookFeed.data)
                    {
                        DateTime createdTime = DateTime.Parse(post.created_time);

                        if (createdTime >= iSince && createdTime <= iUntil)
                        {
                            posts.Add(post);
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (facebookFeed.paging != null)
                    {                        
                        string nextCursor = HttpUtility.ParseQueryString(facebookFeed.paging.next).Get("until");
                        DateTime next = Facebook.DateTimeConvertor.FromUnixTime(nextCursor);

                        if (next >= iSince)
                        {
                            while (facebookFeed.paging != null)
                            {
                                if (facebookFeed.paging.next != string.Empty)
                                {
                                    result = _FBClient.Get(facebookFeed.paging.next);
                                    res = result.ToString();
                                    facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);

                                    foreach (Datum post in facebookFeed.data)
                                    {
                                        DateTime createdTime = DateTime.Parse(post.created_time);

                                        if (createdTime >= iSince && createdTime <= iUntil)
                                        {
                                            posts.Add(post);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    if (facebookFeed.paging != null)
                                    {
                                        //string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("param1");
                                        nextCursor = HttpUtility.ParseQueryString(facebookFeed.paging.next).Get("until");
                                        next = Facebook.DateTimeConvertor.FromUnixTime(nextCursor);
                                    }
                                    else
                                    {
                                        break; // no more posts
                                    }
                                }
                            }
                        }
                        else
                        {
                            break; // posts from next batch are older than we asked
                        }

                    }
                    else
                    {
                        break; // no more posts
                    }

                    if (facebookFeed.paging == null)
                    {
                        break;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("4")) //App level rate limit - sleep 60 minutes
                    {
                        Console.WriteLine("Waiting 60 minutes - App level limit reached");
                        System.Threading.Thread.Sleep(3600000);
                    }
                    else if (ex.Message.Contains("17")) //User level rate limit - sleep 30 minutes
                    {
                        Console.WriteLine("Waiting 30 minutes - User level limit reached");
                        System.Threading.Thread.Sleep(1800000);
                    }
                }
            }

            return posts;
        }

        public List<Datum> GetAllPostsFromFeed(string iUserName)
        {
            List<Datum> posts = new List<Datum>();

            //GET /me/feed
            while (true)
            {
                try
                {
                    object result = _FBClient.Get(string.Format("/{0}/feed", iUserName));
                    string res = result.ToString();

                    FacebookFeed facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);
                    posts.AddRange(facebookFeed.data);

                    while (facebookFeed.paging != null)
                    {
                        if (facebookFeed.paging.next != string.Empty)
                        {
                            result = _FBClient.Get(facebookFeed.paging.next);
                            res = result.ToString();
                            facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);
                            posts.AddRange(facebookFeed.data); 
                        }
                    }

                    if (facebookFeed.paging == null)
                    {
                        break;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("4")) //App level rate limit - sleep 60 minutes
                    {
                        System.Threading.Thread.Sleep(3600000);
                    }
                    else if (ex.Message.Contains("17")) //User level rate limit - sleep 30 minutes
                    {
                        System.Threading.Thread.Sleep(1800000);
                    }
                }
            }

            return posts;
        }

        public List<Datum> GetPostsMatchingRegexPattern(string iUserName, string iRegexPattern, DateTime iSince, DateTime iUntil)
        {
            List<Datum> posts = new List<Datum>();
            DateTime next = iUntil;

            while (next >= iSince)
            {
                try
                {
                    object result = _FBClient.Get(string.Format("/{0}/feed", iUserName));
                    string res = result.ToString();

                    FacebookFeed facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);
                    //posts.AddRange(facebookFeed.data);

                    foreach (Datum post in facebookFeed.data)
                    {
                        DateTime createdTime = DateTime.Parse(post.created_time);

                        if (createdTime >= iSince && createdTime <= iUntil)
                        {
                            if (post.message != null)
                            {
                                if (Regex.IsMatch(post.message, iRegexPattern))
                                {
                                    posts.Add(post);
                                } 
                            }
                        }
                        else
                        {
                            break;
                        }
                    }


                    if (facebookFeed.paging != null)
                    {
                        string nextCursor = HttpUtility.ParseQueryString(facebookFeed.paging.next).Get("until");
                        next = Facebook.DateTimeConvertor.FromUnixTime(nextCursor);

                        if (next >= iSince)
                        {
                            while (facebookFeed.paging != null)
                            {
                                if (facebookFeed.paging.next != string.Empty)
                                {
                                    result = _FBClient.Get(facebookFeed.paging.next);
                                    res = result.ToString();
                                    facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);

                                    foreach (Datum post in facebookFeed.data)
                                    {
                                        DateTime createdTime = DateTime.Parse(post.created_time);

                                        if (createdTime >= iSince && createdTime <= iUntil)
                                        {
                                            if (post.message != null)
                                            {
                                                if (Regex.IsMatch(post.message, iRegexPattern))
                                                {
                                                    posts.Add(post);
                                                } 
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    if (facebookFeed.paging != null)
                                    {
                                        //string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("param1");
                                        nextCursor = HttpUtility.ParseQueryString(facebookFeed.paging.next).Get("until");
                                        next = Facebook.DateTimeConvertor.FromUnixTime(nextCursor);

                                        if (next < iSince)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break; // no more posts
                                    }
                                }
                            }
                        }
                        else
                        {
                            break; // posts from next batch are older than we asked
                        }

                    }
                    else
                    {
                        break; // no more posts
                    }

                    if (facebookFeed.paging == null)
                    {
                        break;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("4")) //App level rate limit - sleep 60 minutes
                    {
                        Console.WriteLine("Waiting 60 minutes - App level limit reached");
                        System.Threading.Thread.Sleep(3600000);
                    }
                    else if (ex.Message.Contains("17")) //User level rate limit - sleep 30 minutes
                    {
                        Console.WriteLine("Waiting 30 minutes - User level limit reached");
                        System.Threading.Thread.Sleep(1800000);
                    }
                    else if (ex.Message.Contains("2"))
                    {
                        System.Threading.Thread.Sleep(2000);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            return posts;
        }
        /*
         * while(true)
         * {
         *      try
         *      {
         *      
         *          GetPosts()
         *      }
         *      catch()
         *      {
         *          check which error it is and wait it out
         *      }
         * }
         * 
         What do i need to do:
         * 1.Look in specific facebook pages for any posts that have a hebrew letter and an apostrophe,
         *      presumably with a rank before it but not only.
         *      so, i should look for something like this : 'ג or this 'אל"מ ג
         *      
         * 2.After finding this pattern in the post, i need to create a tree of users, that commented on that posts
         *      and do that for the comments on their comments and so on until i have all comments in hand
         *      
         *  Should look like this: lets say mako is the origin
         *  mako
         *      -post that has the pattern in it
         *          -comments
         *              -commments on comment of user1
         *                  -comments on comment of user11
         *                  -comments on comment of user11
         *                      -
         *                          -
         *                          -
         *                              -
         *                                  -
         *                                      -
         *              -commments on comment of user2
         *                  -comments on comment of user11
         *                  -comments on comment of user11
         *          -also, need to go over all sharers of post too
         *          
         * 3.Facebook has limits over the usage of its API, so there will be a need to stop the program
         *      for a while (30 minutes\ 60 minutes depending on the exception)
         *      before the program can continue its search
         *      
         * 4.Facebook API doesn't return all the posts at once, so there will be a need to paginate through the results
         */
    }
}
