using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiConnections.Models.DepartureModel;

namespace ApiConnections.Models
{
    public class TrainDepartureModel
    {
        public Trainannouncement Announcements { get; set; }
        public string LocationFullName { get; set; }
        public string ProductInformation { get; set; }
    }
}
