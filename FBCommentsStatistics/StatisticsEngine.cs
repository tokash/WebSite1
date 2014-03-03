using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FBDataEngine;

namespace FBCommentsStatistics
{
    public class StatisticsEngine
    {
        FBResultsFileReader fbReader = new FBResultsFileReader();

        public void CalcStatisticsForFile(string iFilename, Func<Comment, int> iFunc, ref int iCounter)
        {
            List<Post> posts = fbReader.GetPostsFromFile(iFilename);
            int counter = 0;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i].Comments != null && posts[i].Comments.Count > 0)
                {
                    foreach (Comment comment in posts[i].Comments)
                    {
                        iCounter += iFunc(comment);
                    } 
                }
            }
        }

        public void CalcStatisticsForFile(string iFilename, Func<Comment, int> iFunc, ref Dictionary<int, int> iType1, ref Dictionary<int, int> iType2)
        {
            List<Post> posts = fbReader.GetPostsFromFile(iFilename);
            bool flag = true;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i].Comments != null && posts[i].Comments.Count > 0)
                {
                    foreach (Comment comment in posts[i].Comments)
                    {
                        if (comment.Taxonomy != "0")
                        {
                            KeyValuePair<int, int> pair = new KeyValuePair<int,int>();
                            try 
	                        {
                                int result = iFunc(comment);
		                        pair = iType1.Single(x => x.Key == result);
	                        }
	                        catch (Exception)
	                        {
                                flag = false;
	                        }

                            if (!flag)
                            {
                                iType1.Add(iFunc(comment), 1);
                                flag = true;
                            }
                            else
                            {
                                //need to update key
                                int counter = pair.Value;
                                iType1.Remove(pair.Key);
                                iType1.Add(pair.Key, ++counter);
                            }
                        }
                        else
                        {
                            KeyValuePair<int, int> pair = new KeyValuePair<int,int>();
                            try
                            {
                                int result = iFunc(comment);
                                pair = iType2.Single(x => x.Key == result);
                            }
                            catch (Exception)
                            {
                                flag = false;
                            }

                            if (!flag)
                            {
                                iType2.Add(iFunc(comment), 1);
                                flag = true;
                            }
                            else
                            {
                                //need to update key
                                int counter = pair.Value;
                                iType2.Remove(pair.Key);
                                iType2.Add(pair.Key, ++counter);
                            }
                        }
                    }
                }
            }
        }

        public void CalcStatisticsForDirectory(string iDirectoryPath, Func<Comment, int> iFunc, ref int iCounter)
        {
            string[] files = Directory.GetFiles(iDirectoryPath);

            foreach (string file in files)
            {
                CalcStatisticsForFile(file, iFunc, ref iCounter);
            }
        }

        public void CalcStatisticsForDirectory(string iDirectoryPath, Func<Comment, int> iFunc, ref Dictionary<int, int> iType1, ref Dictionary<int, int> iType2)
        {
            string[] files = Directory.GetFiles(iDirectoryPath);

            foreach (string file in files)
            {
                CalcStatisticsForFile(file, iFunc, ref iType1, ref iType2);
            }
        }

        public Func<Comment, int> CountTotalWords = x => x.CommentMessage.Split(' ').Length - 1;

        public void WriteToCSVFile(string iFilename, string[] iHeadlines, Dictionary<int, int> iData)
        {
            string headline = string.Empty;

            //take care of headlines
            for(int i = 0; i < iHeadlines.Length - 1; i++)
            {
                headline += iHeadlines[i] + ",";
            }
            headline += iHeadlines[iHeadlines.Length - 1];

            using (StreamWriter writer = new StreamWriter(iFilename, true))
            {
                writer.WriteLine(headline);
                foreach (KeyValuePair<int, int> record in iData)
	            {
                    string line = string.Format("{0},{1}", record.Key, record.Value);
                    writer.WriteLine(line);
	            }
            }
        }
    }
}
