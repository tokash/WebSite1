using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Facebook;
using FacebookCrawler.FBObjects;
using TextFileWriter;
using System.Diagnostics;

namespace FacebookCrawler
{
    class Program
    {
        private static string AppID = "630904423618987";
        private static string AppSecret = "485456384f450bde62bc9eacbfc2c316";
        private static string ClientToken = "35673ccf126aaf11244cbd1b9b813c84";
        private static string AppToken = "630904423618987|NTKVsaFHzzi4E-sD2Rodb8ECSCE";
        private static string UserToken = "CAAI9zeIOmasBAA7uzNklhZC8xSGGTZBHUjJZBYkLDgBtn2AXLxDaZCSs0NshY68b7OLuVZAUZCUy71gMIg29jlfqdZBb3wO9kRBFF1td77hxyqkOXMmxKTPh65RtfkCNlDoxx8cPzhx08zxsrcRIkZBjbDJj341kTJOPk88baTjvZBk7MSZCwH9w6jcKW3dAm2ZA3kOCQzlEGvNcFpijYsJiwD6I0K1M9m7SzTClvmZAsfzmnQZDZD";
        

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            FaceBookAPI fb = new FaceBookAPI();

            //Read FBPages file from app.config
            string FBPageList = ConfigurationManager.AppSettings["FBPageList"];
            List<string> FBPagesToTraverse = null;

            string FBPagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FBPageList);
            Console.WriteLine(string.Format("Current running directory: {0}", FBPagePath));
            using (StreamReader sr = new StreamReader(FBPagePath))
            {
                String line = sr.ReadToEnd();
                FBPagesToTraverse = line.Split('\n').ToList<string>();
            }

            for (int i = 0; i < FBPagesToTraverse.Count; i++)
            {
                FBPagesToTraverse[i] = FBPagesToTraverse[i].Trim();
            }

            int j = 0;

            foreach (string feed in FBPagesToTraverse)
            {
                Thread t = new Thread(() => fb.GetAllFeedInformation(feed));
                t.Name = String.Format("{0}_{1}", feed, Guid.NewGuid());

                t.Start();
            }

            sw.Stop();

            Console.WriteLine(sw.Elapsed);
            //Console.ReadLine();          
        }

        
    }
}
