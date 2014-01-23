using System;
using System.Collections.Generic;
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

            fbReader.ConvertToCSV("press release_graph pilot_12.01.14.txt");
        }
    }
}
