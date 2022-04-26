using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiConnections.Models
{
    public class StationMessages
    {

        public class MessagesRootobject
        {
            public RESPONSE RESPONSE { get; set; }
        }

        public class RESPONSE
        {
            public RESULT[] RESULT { get; set; }
        }

        public class RESULT
        {
            public Trainmessage[] TrainMessage { get; set; }
        }

        public class Trainmessage
        {
            public string ExternalDescription { get; set; }
            public string ReasonCodeText { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime LastUpdateDateTime { get; set; }
        }

    }
}
