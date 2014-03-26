using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FBDataEngine;

namespace FBCommentsStatistics
{
    
    //Read file
    //Go over file for specific statistic (counters mostly)
    //public void CalcStatistic(string iFilename, Func<> StatFuncPtr)
    //Foreach file read, seperate between special and non-special comments (special = ##, rest - non-special)

    class Program
    {
        static void Main(string[] args)
        {
            List<CommentStatistics> commentStatistics = new List<CommentStatistics>();
            List<KeyValuePair<string, Func<Comment, int>>> funcList = new List<KeyValuePair<string, Func<Comment, int>>>();
            StatisticsEngine se = new StatisticsEngine();
            

            funcList.Add(new KeyValuePair<string,Func<Comment,int>>("NumWords", new Func<Comment,int>(se.CountTotalWords)));
            funcList.Add(new KeyValuePair<string, Func<Comment, int>>("ExclamationMarks", new Func<Comment, int>(se.CountExclamtionMarks)));
            funcList.Add(new KeyValuePair<string, Func<Comment, int>>("Smilies", new Func<Comment, int>(se.CountSmilies)));
            funcList.Add(new KeyValuePair<string, Func<Comment, int>>("3Dots", new Func<Comment, int>(se.Count3Dots)));
            funcList.Add(new KeyValuePair<string, Func<Comment, int>>("Winks", new Func<Comment, int>(se.CountWinks)));
            funcList.Add(new KeyValuePair<string, Func<Comment, int>>("3SameLettersOrMore", new Func<Comment, int>(se.Count3LettersOrMore)));

            //string directoryPath = @"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data_other relevant";
            //string directoryPath = @"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data for graphs_UDL";
            //string directoryPath = @"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data for graphs no UDL";
            string directoryPath = @"C:\Users\Tokashyo\Documents\Visual Studio 2012\Projects\WebSite1\FBCommentsStatistics\bin\Debug\test";
            
            string statsFilename = string.Format("{0}_Stats_{1}.csv", Path.GetFileName(directoryPath), DateTime.Now.ToString("dd.MM.yyyy.HH.mm.ss.ffff"));

            se.CalcStatisticsForDirectory(directoryPath, funcList, ref commentStatistics);
            //se.WriteDataToCSV(statsFilename, new string[]{"type", "classification", "text", "date", "hour", "rank", "num words", "num !", "num :)", "3Dots", "Winks", "3 Letters or more"}, commentStatistics);
            se.WriteDataToCSV(statsFilename, new string[] { "Type", "Classification", "Text", "Date", "Hour", "Rank" }, commentStatistics);

            //se.CalcStatisticsForDirectory(directoryPath, new Func<Comment, int>(se.CountTotalWords), ref commentStatistics, new string[] { "ExclamationMarks" });

            //se.CalcStatisticsForFile(@"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data for graphs_UDL\c.s_1.1_for graph.docx", new Func<Comment, int>(se.CountTotalWords), ref commentStatistics, "NumWords");


            //se.CalcStatisticForFile(@"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data for graphs_UDL\c.s_1.1_for graph.docx", new Func<Comment, int> (se.CountWords), ref counter);

            //se.CalcStatisticsForDirectory(directoryPath, new Func<Comment, int>(se.CountTotalWords), ref type1, ref type2);
            //se.WriteToCSVFile("UDL.csv", new string[]{"NumWords", "Comments"}, type1.OrderBy(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value));
            //se.WriteToCSVFile("nonUDL.csv", new string[] { "NumWords", "Comments" }, type2.OrderBy(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
