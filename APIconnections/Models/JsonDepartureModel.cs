using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiConnections.Models
{
    public class JsonDepartureModel
    {
        public class DepartureRootobject
        {
            public RESPONSE RESPONSE { get; set; }
        }
        public class RESPONSE
        {
            public RESULT[] RESULT { get; set; }
        }
        public class RESULT
        {
            public Trainannouncement[] TrainAnnouncement { get; set; }
        }
        public class Trainannouncement
        {
            public DateTime AdvertisedTimeAtLocation { get; set; }
            public string AdvertisedTrainIdent { get; set; }
            public Tolocation[] ToLocation { get; set; }
            public string TrackAtLocation { get; set; }
        }
        public class Tolocation
        {
            public string LocationName { get; set; }
            public int Priority { get; set; }
            public int Order { get; set; }
        }
    }
}
