using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineInterfaceFileDrop
{
    public class Initialize
    {
        /// <summary>
        /// A path may contain 'special' Windows folders, such as
        /// (where we use curly bracket to be angle bracket):
        /// {ApplicationData} - resolves to AppData in Windows 10
        /// These must be delimited with the greater-than and less-than angle brackets and
        /// if used, can only be at the beginning of the path.
        /// </summary>
        /// <param name="rawPath"></param>
        /// <returns></returns>
        public static string ResolvePath(string rawPath)
        {
            string resolvedPath = "";
            try
            {
                int startIndex = rawPath.IndexOf('<');
                if (startIndex > -1)
                {
                    if (startIndex != 0)
                        throw new ApplicationException($"Special folders can only be at the beginning of the path. RawPath={rawPath}");

                    int stopIndex = rawPath.IndexOf('>');
                    int len = stopIndex - startIndex - 1;
                    if (len <= 0)
                        throw new ApplicationException($"RawPath={rawPath} No special folder found. Bad syntax.");

                    string folderName = rawPath.Substring(startIndex + 1, len);
                    if (!Environment.SpecialFolder.TryParse(folderName, out Environment.SpecialFolder enumSpecial))
                    {
                        throw new ApplicationException($"Unknown special folder={folderName}");
                    }

                    string specialPath = Environment.GetFolderPath(enumSpecial, Environment.SpecialFolderOption.Create);
                    resolvedPath = specialPath + rawPath.Substring(stopIndex + 1);

                }
                else
                {
                    resolvedPath = rawPath;
                }

                if (!Directory.Exists(resolvedPath))
                    Directory.CreateDirectory(resolvedPath);

                return resolvedPath;

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot resolve Path={rawPath}. Err={ex.Message}");
            }
        }


    }
}
