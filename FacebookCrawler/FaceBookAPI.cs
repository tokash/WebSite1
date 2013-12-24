﻿using System;
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
using System.Threading;
using System.Diagnostics;

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
        static Semaphore _Semaphore = new Semaphore(3, 3);

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

        public List<Datum> GetPostsMatchingRegexPattern(string iUserName, string iRegexPattern, DateTime iSince, DateTime iUntil, ref List<Datum> oTotalPosts)
        {
            List<Datum> posts = new List<Datum>();
            DateTime next = iUntil;

            while (next >= iSince)
            {
                try
                {
                    object result = _FBClient.Get(string.Format("/{0}/feed", iUserName));
                    System.Threading.Thread.Sleep(1000);
                    string res = result.ToString();

                    FacebookFeed facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);

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
                                oTotalPosts.Add(post);
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
                                    System.Threading.Thread.Sleep(1000);
                                    res = result.ToString();
                                    facebookFeed = JsonConvert.DeserializeObject<FacebookFeed>(res);
                                    oTotalPosts.AddRange(facebookFeed.data);

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
                                                oTotalPosts.Add(post);
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
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                }
            }

            return posts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPost"> The post object</param>
        /// <param name="iNumberOfCommentsToGet">How many comments to get for the post, default = 0 (0 = get all comments)</param>
        /// <returns></returns>
        public List<Datum4> GetMoreCommentsForPost(Datum iPost, int iNumberOfCommentsToGet = 0)
        {
            List<Datum4> comments = new List<Datum4>();

            int i = 0;
            string nextPage = iPost.comments.paging.next;

            try
            {
                if (iNumberOfCommentsToGet == 0)
                {
                    while (nextPage != null)
                    {
                        if (iPost.comments.paging != null)
                        {
                            if (nextPage != null && nextPage != string.Empty)
                            {
                                object result = _FBClient.Get(nextPage);
                                System.Threading.Thread.Sleep(1000);
                                string res = result.ToString();
                                Comments currCommentsBatch = JsonConvert.DeserializeObject<Comments>(res);

                                comments.AddRange(currCommentsBatch.data);
                                nextPage = currCommentsBatch.paging.next;
                            }
                        }
                    }
                }
                else
                {
                    while (i < iNumberOfCommentsToGet && nextPage != null)
                    {
                        object result = _FBClient.Get(nextPage);
                        System.Threading.Thread.Sleep(1000);
                        string res = result.ToString();
                        Comments currCommentsBatch = JsonConvert.DeserializeObject<Comments>(res);

                        comments.AddRange(currCommentsBatch.data);
                        nextPage = currCommentsBatch.paging.next;

                        i += currCommentsBatch.data.Count;
                    }
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

            return comments;
        }

        public void GetFeedInformation(string iFeedName, DateTime iStartTime, DateTime iEndTime)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("{0} is waiting in line...", Thread.CurrentThread.Name);
            _Semaphore.WaitOne();
            Console.WriteLine("{0} is Working...", Thread.CurrentThread.Name);
            List<Datum> totalPosts = new List<Datum>();

            List<Datum> posts = GetPostsMatchingRegexPattern(iFeedName, "[\u0591-\u05F4][\u0591-\u05F4]\"[\u0591-\u05F4] [\u0591-\u05F4]\'", iStartTime, iEndTime, ref totalPosts);
            //List<Datum> posts = GetPostsMatchingRegexPattern(iFeedName, "", iStartTime, iEndTime, ref totalPosts);
            
            foreach (Datum post in posts)
            {
                if (post.comments != null)
                {
                    post.comments.data.AddRange(GetMoreCommentsForPost(post));
                }
            }

            string results = string.Empty;
            foreach (Datum post in posts)
            {
                results += FBResultFormatter.FormatFBPost(post, iFeedName);
            }

            FBResultFormatter.FormatFBStats(totalPosts.Count, posts.Count, iStartTime, iEndTime, ref results);

            string fileRepository = ConfigurationManager.AppSettings["ResultsLocation"];

            TextFileWriter.TextFileWriter.WriteTextFile(string.Format("{0}{1}.txt",fileRepository, iFeedName), results);
            sw.Stop();
            Console.WriteLine("{0} is done, total work time for thread: {1}", Thread.CurrentThread.Name, sw.Elapsed);

            _Semaphore.Release();
        }
    }
}