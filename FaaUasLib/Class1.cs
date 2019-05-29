using System;
using System.IO;
using Newtonsoft.Json;

namespace FaaUasLib
{
    public class FaaUas
    {
        private string fileContents;
        private FaaUasLib.Models.FascilityMap fascilityMap_;

        public FaaUasLib.Models.FascilityMap fascilityMap => fascilityMap_;

        public FaaUas()
        {
        }

        public void readFile(string fileName)
        {
            fileContents = File.ReadAllText(fileName);
        }

        public void getData()
        {
            //readFile("Y:\\Data\\Telemething\\LAANC\\faa_uas_fascilitymap_data_v2_latlon.json");  //old
            readFile("Data\\faa_uas_fascilitymap_data_v2_latlon.json");                           // implemented on uwp, not iOS or Android
            fascilityMap_ = JsonConvert.DeserializeObject<FaaUasLib.Models.FascilityMap>(fileContents);
        }
    }
}
