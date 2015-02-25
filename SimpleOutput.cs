using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helpers
{
    public class SimpleOutput
    {
        public static void PrintArrayToText<T>(string FileName, T[] Data, Func<T, string> WriteLine = null, bool Overwrite = false)
        {
            if (WriteLine == null)
                WriteLine = new Func<T, string>((o) => o.ToString());

            if (File.Exists(FileName) && !Overwrite)
            {
                int d = 1, i = FileName.LastIndexOf('.') + 1;
                FileName.Insert(FileName.LastIndexOf('.'), "_1");
                while (File.Exists(FileName))
                {
                    FileName.Remove(i, d.ToString().Length);
                    FileName.Insert(i, (++d).ToString());
                }
            }
            else if (Overwrite)
                File.Delete(FileName);

            TextWriter Writer = new StreamWriter(FileName);

            foreach (T o in Data)
                Writer.WriteLine(WriteLine(o));

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
        }
    }
}
