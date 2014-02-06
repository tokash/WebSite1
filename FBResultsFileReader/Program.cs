using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBResultsFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            FBResultsFileReader fbReader = new FBResultsFileReader();

            string directoryPath = @"D:\Dropbox\BIU Group\Qualitative research\c.s data for graphs";
            string[] files = Directory.GetFiles(directoryPath);

            foreach (var filename in files)
            {
                fbReader.ConvertToCSV(filename);
            }
        }
    }
}
