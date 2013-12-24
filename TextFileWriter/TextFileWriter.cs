using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFileWriter
{
    public class TextFileWriter
    {
        public static void WriteTextFile(string iFullPath, string iFileData)
        {
            if (Directory.Exists(Path.GetDirectoryName(iFullPath)))
            {
                using (StreamWriter writer = new StreamWriter(iFullPath, true))
                {
                    writer.WriteLine(iFileData);
                }
            }
            else
            {
                throw new Exception("Path doesn't exist");
            }
        }
    }
}
