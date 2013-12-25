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

    public class FBResultFormatter
    {
        public static string FormatFBPost(Datum iFBPost,  string iFeedName)
        {
            string result = string.Empty;

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
                        result += String.Format("Commenter: {0} Wrote:{1} \n", comment.from.name, comment.message);
                    }
                }
            }

            result += "\n----------------------------------------------------------------------------------------------------\n";

            return result;
        }

        public static void FormatFBStats(int iTotalPosts, int iMeaningfullPosts, DateTime iStart, DateTime iEnd, ref string oResult)
        {
            oResult += string.Format("\nTotal posts: {0}\n", iTotalPosts);
            oResult += string.Format("Meaningfull posts out of total posts: {0}\n", iMeaningfullPosts);

            TimeSpan informationTime = (iEnd - iStart);
            oResult += string.Format("Information taken from {0} to {1}, all in total: {2} days, {3} hours, {4} minutes, {5} seconds \n", iStart, iEnd, informationTime.Days, informationTime.Hours, informationTime.Minutes, informationTime.Seconds);
        }
    }
}
