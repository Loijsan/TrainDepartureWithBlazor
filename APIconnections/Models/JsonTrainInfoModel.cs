using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiConnections.Models
{
    public class JsonTrainInfoModel
    {

        public class InfoRootobject
        {
            public RESPONSE RESPONSE { get; set; }
        }

        public class RESPONSE
        {
            public RESULT[] RESULT { get; set; }
        }

        public class RESULT
        {
            public InfoTrainannouncement[] TrainAnnouncement { get; set; }
        }

        public class InfoTrainannouncement
        {
            public string InformationOwner { get; set; }
            public string[] ProductInformation { get; set; }
        }

    }
}
