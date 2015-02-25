using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helpers
{
    public static class FileHelpers
    {
        public static string IterFileName(string FileName, int MaxTries = 1000)
        {
            for(int i = 1; i < MaxTries; i++)
            {
                var fileName = FileName.QuickFormat(i);
                if (!File.Exists(fileName))
                    return fileName;
            }
            throw new OperationCanceledException("Couldn't find a valid iterated file name in the given number of tries (Did you forget to provide a {0} for the number to go in?)");
        }
    }
}
