using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiConnections.Models
{
    public class JsonStationNameModel
    {
        public class NameRootobject
        {
            public RESPONSE RESPONSE { get; set; }
        }
        public class RESPONSE
        {
            public RESULT[] RESULT { get; set; }
        }
        public class RESULT
        {
            public TrainstationName[] TrainStation { get; set; }
        }
        public class TrainstationName
        {
            public string AdvertisedLocationName { get; set; }
        }
    }
}
