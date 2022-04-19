using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiConnections.Models
{
    public class JsonStationObject
    {
        public class Rootobject
        {
            public RESPONSE RESPONSE { get; set; }
        }

        public class RESPONSE
        {
            public RESULT[] RESULT { get; set; }
        }

        public class RESULT
        {
            public Trainstation[] TrainStation { get; set; }
        }

        public class Trainstation
        {
            public string AdvertisedLocationName { get; set; }
            public string AdvertisedShortLocationName { get; set; }
            public string LocationSignature { get; set; }
        }
    }
}
