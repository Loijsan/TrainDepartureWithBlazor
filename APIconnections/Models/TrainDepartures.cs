using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiConnections.Models.DepartureModel;

namespace ApiConnections.Models
{
    public class TrainDepartures
    {
        public Trainannouncement Announcements { get; set; }
        public string LocationName { get; set; }
    }
}
