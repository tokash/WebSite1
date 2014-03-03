using System;
using System.Collections.Generic;
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
            int counter1 = 0;
            int counter2 = 0;
            Dictionary<int, int> type1 = new Dictionary<int, int>();
            Dictionary<int, int> type2 = new Dictionary<int, int>();

            string directoryPath = @"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data_other relevant";
            StatisticsEngine se = new StatisticsEngine();
            //se.CalcStatisticForFile(@"D:\Dropbox\BIU Group\Qualitative research\Case Study for Graphs\c.s data for graphs_UDL\c.s_1.1_for graph.docx", new Func<Comment, int> (se.CountWords), ref counter);

            se.CalcStatisticsForDirectory(directoryPath, new Func<Comment, int>(se.CountTotalWords), ref type1, ref type2);
            se.WriteToCSVFile("UDL.csv", new string[]{"NumWords", "Comments"}, type1.OrderBy(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value));
            se.WriteToCSVFile("nonUDL.csv", new string[] { "NumWords", "Comments" }, type2.OrderBy(pair => pair.Key).ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
