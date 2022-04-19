using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using static ApiConnections.Models.JsonStationObject;

namespace ApiConnections.Data
{
    public class TrainStationFinder
    {
        private readonly IConfiguration _configuration;

        public TrainStationFinder(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<List<Trainstation>> GetStations(string stationName)
        {
            //string token = _configuration.GetSection("TrainToken").ToString();
            // TODO! Lösa så att jag får in config i denna...

            var token = "bb89bc88865e4784b501081c6d37b0dc";
            // TODO! Byta token när jag är klar med denna!

            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{token}'/>" +
                                     "<QUERY objecttype='TrainStation' schemaversion='1'>" +
                                         "<FILTER>" +
                                             $"<LIKE name='AdvertisedLocationName' value= '{stationName}' />" +
                                             "<EQ name='Advertised' value='true' />" +
                                         "</FILTER>" +
                                         "<INCLUDE>AdvertisedLocationName</INCLUDE >" +
                                         "<INCLUDE>AdvertisedShortLocationName</INCLUDE>" +
                                         "<INCLUDE>LocationSignature</INCLUDE>" +
                                     "</QUERY>" +
                                  "</REQUEST>";
            // Gör så att den returnerar en sträng istället
            //List<Trainstation> stations = await ApiPostCaller(requestBody);
            //return stations;
            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                List<Trainstation> resultList = JsonStationExtractor(result);
                return resultList;
            }
            return null;
        }

        public async Task<string> ApiPostCaller(string requestBody)
        {
            StringContent stringContent = new(requestBody);

            using (var client = new HttpClient())
            {
                // This is a post method and it can be used with different request bodies :D

                client.BaseAddress = new Uri("https://api.trafikinfo.trafikverket.se/v2/data.json");

                var postTask = client.PostAsync(client.BaseAddress, stringContent);
                postTask.Wait();

                if (postTask.Result.IsSuccessStatusCode)
                {
                    var result = await postTask.Result.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    Console.WriteLine(postTask.Result.ReasonPhrase);
                    // TODO - detta sa Carl inte var ett bra sätt att koda, man ska inte räkna med nullvärden
                    return null;
                }
            }
        }
        private List<Trainstation> JsonStationExtractor(string result)
        {
            var jsonStation = JsonConvert.DeserializeObject<Rootobject>(result);
            var stationsList = jsonStation.RESPONSE.RESULT;
            List<Trainstation> trainStationList = new();

            // Detta är inte en klockren lösning!
            foreach (var stations in stationsList)
            {
                //Console.WriteLine("Stations that match your searchcriteria: ");
                foreach (var station in stations.TrainStation)
                {
                    //Console.WriteLine(station.AdvertisedLocationName);
                    trainStationList.Add(station);
                }
            }
            return trainStationList;
        }
    }
}
