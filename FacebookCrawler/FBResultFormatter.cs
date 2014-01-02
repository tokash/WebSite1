using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacebookCrawler.FBObjects;

namespace FacebookCrawler
{
    //Need to create a text file with the following details:
    /*
     * 1.Feed
     * 2.Post text
     * 3.Timestamp
     * 4.List of commenters
     * 5.How many posts were checked
     * 6.Meaningfull posts out of checked ones
     * 7.Time span of search
    */

    class FBResultFormatter
    {
        FaceBookAPI _FBAPIWrapper;

        internal FBResultFormatter(FaceBookAPI FBAPIWrapper)
        {
            _FBAPIWrapper = FBAPIWrapper;
        }

        public string FormatFBPost(Datum iFBPost, string iFeedName)
        {
            string result = string.Empty;
            List<FBBasicUser> commenters = new List<FBBasicUser>();

            result += "----------------------------------------------------------------------------------------------------\n";
            result += String.Format("Feed: {0}\n", iFeedName);
            result += String.Format("Post message: {0}\n", iFBPost.message);
            result += String.Format("Created at: {0}\n\n", iFBPost.created_time);

            result += "Commenters:\n";
            Comments comments = iFBPost.comments;
            if (comments != null)
            {
                foreach (Datum4 comment in comments.data)
                {
                    if (comment.from.name != null)
                    {
                        FBBasicUser currUserInfo = null;

                        if (commenters.Exists(i => i.id == comment.from.id))
                        {
                            currUserInfo = commenters.Find(i => i.id == comment.from.id);
                        }
                        else
                        {
                            currUserInfo = _FBAPIWrapper.GetUserInformationByID(comment.from.id);

                            if (currUserInfo != null)
                            {
                                commenters.Add(currUserInfo);
                            }
                        }

                        result += String.Format("Commenter: {0} Wrote:{1} \n", comment.from.name, comment.message);

                        string commenterLink = string.Empty;
                        if (currUserInfo != null)
                        {
                            commenterLink = currUserInfo.link;
                            result += String.Format("Commenter link: {0}\n\n", commenterLink);
                        }
                        
                    }
                }
            }

            result += "\n----------------------------------------------------------------------------------------------------\n";

            return result;
        }

        public void FormatFBStats(int iTotalPosts, int iMeaningfullPosts, DateTime iStart, DateTime iEnd, DateTime iLastPost, ref string oResult)
        {
            oResult += string.Format("\nTotal posts: {0}\n", iTotalPosts);
            oResult += string.Format("Meaningfull posts out of total posts: {0}\n", iMeaningfullPosts);

            TimeSpan informationTime = (iEnd - iStart);
            oResult += string.Format("Information taken from {0} to {1}, all in total: {2} days, {3} hours, {4} minutes, {5} seconds \n", iStart, iEnd, informationTime.Days, informationTime.Hours, informationTime.Minutes, informationTime.Seconds);
            oResult += string.Format("Last post date: {0}\n", iLastPost);
        }
    }
}
