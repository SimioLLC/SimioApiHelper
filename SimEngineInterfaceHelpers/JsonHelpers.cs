using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SimEngineInterfaceHelpers
{
    public static class JsonHelpers
    {
        /// <summary>
        /// Serialize data to a text file.
        /// </summary>
        /// <param name="dataToSerialize"></param>
        /// <param name="filePath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool SerializeToFile<T>(string filePath, T dataToSerialize, bool appendToFile, out string explanation)

        {
            explanation = "";
            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = 100000000; // magic number. 8 naughts
                var jsonString = jss.Serialize(dataToSerialize);

                if (appendToFile)
                {
                    File.AppendAllText(filePath, jsonString);

                }
                else
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    File.WriteAllText(filePath, $"{jsonString}\n");
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"File={filePath} Err={ex.Message}";
                return false;
            }
        } // method

        /// <summary>
        /// Deserialize data from a text file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="deserializedData"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool DeserializeFromFile<T>(string filePath, out T deserializedData, out string explanation)

        {
            explanation = "";
            deserializedData = default(T);

            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();

                if (!File.Exists(filePath))
                {
                    explanation = $"No such file={filePath}";
                    return false;

                }
                else
                {
                    string jsonString = File.ReadAllText(filePath);

                    jss.MaxJsonLength = 100000000;
                    deserializedData = (T)jss.Deserialize(jsonString, typeof(T));
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"File={filePath} Err={ex}";
                return false;
            }
        } // method

        /// <summary>
        /// Serialize data to a text file.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="deserializedData"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool DeserializeFromString<T>(string jsonString, out T deserializedData, out string explanation)

        {
            explanation = "";
            deserializedData = default(T);

            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();

                deserializedData = (T)jss.Deserialize(jsonString, typeof(T));

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Err={ex} String={jsonString}";
                return false;
            }
        } // method

    }
}

