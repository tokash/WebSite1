using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public void CalcStatisticsForFile(string iFilename, Func<Comment, int> iFunc, ref List<CommentStatistics> iCommentStatistics, string iStatisticType)
        {
            List<Post> posts = fbReader.GetPostsFromFile(iFilename);
            bool flag = true;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i].Comments != null && posts[i].Comments.Count > 0)
                {
                    foreach (Comment comment in posts[i].Comments)
                    {
                        CommentStatistics curr = new CommentStatistics();
                        string type = string.Empty;

                        curr._Comment = comment;

                        if (comment.Taxonomy != "0")
                        {
                            curr._Type = "UDL";
                        }
                        else
                        {
                            curr._Type = "non-UDL";
                        }

                        PropertyStatistic ps = new PropertyStatistic() { Name = iStatisticType, Statistic = iFunc(comment)};
                            
                        curr._Properties.Add(ps);

                        iCommentStatistics.Add(curr);
                    }
                }
            }
        }

        public void CalcStatisticsForFile(string iFilename, List<KeyValuePair<string, Func<Comment, int>>> iFuncList, ref List<CommentStatistics> iCommentStatistics)
        {
            List<Post> posts = fbReader.GetPostsFromFile(iFilename);
            bool flag = true;

            for (int i = 0; i < posts.Count; i++)
            {
                if (posts[i].Comments != null && posts[i].Comments.Count > 0)
                {
                    for (int j = 0; j < posts[i].Comments.Count; j++ )
                    {
                        CommentStatistics curr = new CommentStatistics();
                        string type = string.Empty;

                        curr._Comment = posts[i].Comments[j];

                        if (posts[i].Comments[j].Taxonomy != "0")
                        {
                            curr._Type = "UDL";
                        }
                        else
                        {
                            curr._Type = "non-UDL";
                        }

                        curr._Rank = j + 1;

                        foreach (KeyValuePair<string, Func<Comment, int>> item in iFuncList)
                        {
                            PropertyStatistic ps = new PropertyStatistic() { Name = item.Key, Statistic = item.Value(posts[i].Comments[j]) };

                            curr._Properties.Add(ps);
                        }

                        iCommentStatistics.Add(curr);
                    }
                }
            }
        }

        public void CalcStatisticsForDirectory(string iDirectoryPath, List<KeyValuePair<string, Func<Comment, int>>> iFuncList, ref List<CommentStatistics> iCommentStatistics)
        {
            string[] files = Directory.GetFiles(iDirectoryPath);

            foreach (string file in files)
            {
                CalcStatisticsForFile(file, iFuncList, ref iCommentStatistics);
            }
        }

        public void CalcStatisticsForDirectory(string iDirectoryPath, Func<Comment, int> iFunc, ref List<CommentStatistics> iCommentStatistics, string[] iWantedProperties)
        {
            string[] files = Directory.GetFiles(iDirectoryPath);

            foreach (string property in iWantedProperties)
	        {
		        foreach (string file in files)
                {
                    CalcStatisticsForFile(file, iFunc, ref iCommentStatistics, property);
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

        public Func<Comment, int> CountExclamtionMarks = x => x.CommentMessage.Count(y => y == '!');

        public Func<Comment, int> CountSmilies = x => Regex.Matches(x.CommentMessage, ":\\)").Count;

        public Func<Comment, int> CountWinks = x => Regex.Matches(x.CommentMessage, ";\\)").Count;

        public Func<Comment, int> Count3Dots = x => Regex.Matches(x.CommentMessage, "\\.\\.\\.").Count;

        //[\u05D0-\u05F4]
        //Aleph - u05D1
        //Bet - u05D2
        //Gimel - u05D3
        //Dalet - u05D4
        //He - u05D5
        //Vav - u05D6
        //Zayin - u05D7
        //Het - u05D8
        //Tet - u05D9
        //Yod - u05DA
        //Chaf - u05DB
        //Lamed - u05DC
        //Mem - u05DE
        //Nun - u05E0
        //Samech - u05E1
        //Ayin - u05E2
        //Peh - u05E4
        //Zadik - u05E6
        //Kof - u05E7
        //Reish - u05E8
        //Shin - u05E9
        //Tav - u05EB
        public Func<Comment, int> Count3LettersOrMore = x => Regex.Matches(x.CommentMessage, "(\\w)\\1{2,}").Count;

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

        public void WriteDataToCSV(string iFilename, string[] iHeadlines, List<CommentStatistics> iCommentStatistics)
        {
            string headline = string.Empty;

            //take care of headlines
            for (int i = 0; i < iHeadlines.Length; i++)
            {
                headline += iHeadlines[i] + ",";
            }

            for (int i = 0; i < iCommentStatistics[0]._Properties.Count - 1; i++)
            {
                headline += iCommentStatistics[0]._Properties[i].Name + ",";
            }
            headline += iCommentStatistics[0]._Properties[iCommentStatistics[0]._Properties.Count - 1].Name;

            using (StreamWriter writer = new StreamWriter(iFilename, true))
            {
                writer.WriteLine(headline);
                foreach (CommentStatistics record in iCommentStatistics)
                {
                    string baseline = string.Format("{0},{1},{2},{3},{4},{5}",
                        record._Type,
                        record._Comment.Taxonomy.Replace(',', ';'),
                        record._Comment.CommentMessage.Replace(',', ' '),
                        record._Comment.CommentDate.Date.ToString("d"),
                        record._Comment.CommentDate.TimeOfDay,
                        record._Rank);

                    string statProperties = string.Empty;
                    foreach (PropertyStatistic item in record._Properties)
                    {
                        statProperties += item.Statistic + ",";
                    }

                    string line = baseline + "," +statProperties;
                    writer.WriteLine(line);
                }
            }
        }
    }
}
