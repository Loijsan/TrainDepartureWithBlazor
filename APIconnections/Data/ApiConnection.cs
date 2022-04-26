using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using static ApiConnections.Models.DepartureModel;
using static ApiConnections.Models.JsonDepartureModel;
using static ApiConnections.Models.JsonStationNameModel;
using static ApiConnections.Models.JsonStationObject;
using static ApiConnections.Models.StationMessages;

namespace ApiConnections.Data
{
    public class ApiConnection
    {
        private string Token = "bb89bc88865e4784b501081c6d37b0dc";

        public async Task<List<Trainstation>> GetStationsAsync(string stationName)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{Token}'/>" +
                                     "<QUERY objecttype='TrainStation' schemaversion='1' limit='10'>" +
                                         "<FILTER>" +
                                             $"<LIKE name='AdvertisedLocationName' value= '{stationName}' />" +
                                             "<EQ name='Advertised' value='true' />" +
                                         "</FILTER>" +
                                         "<INCLUDE>AdvertisedLocationName</INCLUDE >" +
                                         "<INCLUDE>AdvertisedShortLocationName</INCLUDE>" +
                                         "<INCLUDE>LocationSignature</INCLUDE>" +
                                     "</QUERY>" +
                                  "</REQUEST>";

            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                List<Trainstation> resultList = JsonStationExtractor(result);
                return resultList;
            }
            return null;
        }
        public async Task<List<Trainannouncement>> GetDeparturesAsync(string locationSignature)
        {
            // TODO! - Hade hellre begränsat den i tid än i antal...
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{Token}' />" +
                                    "<QUERY objecttype='TrainAnnouncement' schemaversion='1.3' orderby='AdvertisedTimeAtLocation'>" +
                                    "<FILTER>" +
                                    "<AND>" +
                                    "<EQ name='ActivityType' value='Avgang' />" +
                                    $"<EQ name='LocationSignature' value='{locationSignature}' />" +
                                    "<OR>" +
                                          "<AND>" +
                                                "<GT name='AdvertisedTimeAtLocation' value='$dateadd(-00:05:00)' />" +
                                                "<LT name='AdvertisedTimeAtLocation' value='$dateadd(2:00:00)' />" +
                                          "</AND>" +
                                          "<AND>" +
                                                "<LT name='AdvertisedTimeAtLocation' value='$dateadd(00:30:00)' />" +
                                                "<GT name='EstimatedTimeAtLocation' value='$dateadd(-00:15:00)' />" +
                                          "</AND>" +
                                             "</OR>" +
                                          "</AND>" +
                                    "</FILTER>" +
                                    "<INCLUDE>AdvertisedTrainIdent</INCLUDE>" +
                                    "<INCLUDE>AdvertisedTimeAtLocation</INCLUDE>" +
                                    "<INCLUDE>Canceled</INCLUDE>" +
                                    "<INCLUDE>EstimatedTimeAtLocation</INCLUDE>" +
                                    "<INCLUDE>TrackAtLocation</INCLUDE>" +
                                    "<INCLUDE>ToLocation</INCLUDE>" +
                                    "<INCLUDE>Deviation</INCLUDE>" +
                              "</QUERY>" +
                        "</REQUEST>";

            // Jag vill att den först frågar ApiPostCaller
            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                List<Trainannouncement> departureList = JsonDepartureExtractor(result);
                return departureList;
            }
            return null;
        }
        public async Task<TrainstationName> GetStationNameAsync(string locationSignature)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{Token}'/>" +
                                     "<QUERY objecttype='TrainStation' schemaversion='1'>" +
                                         "<FILTER>" +
                                             $"<EQ name='LocationSignature' value= '{locationSignature}' />" +
                                             "<EQ name='Advertised' value='true' />" +
                                         "</FILTER>" +
                                         "<INCLUDE>AdvertisedLocationName</INCLUDE >" +
                                     "</QUERY>" +
                                  "</REQUEST>";

            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                TrainstationName stationName = JsonNameExtractor(result);
                //var stationName = JsonConvert.DeserializeObject<NameRootobject>(result);
                return stationName;
            }
            //Console.WriteLine("No name found");
            return null;
        }
        public async Task<List<Trainmessage>> GetStationMessagesAsync(string locationSignature)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{Token}'/>" +
                                     "<QUERY objecttype='TrainMessage' schemaversion='1.3'>" +
                                         "<FILTER>" +
                                             $"<EQ name='AffectedLocation' value= '{locationSignature}' />" +
                                         "</FILTER>" +
                                         "<INCLUDE>StartDateTime</INCLUDE >" +
                                         "<INCLUDE>LastUpdateDateTime</INCLUDE >" +
                                         "<INCLUDE>ExternalDescription</INCLUDE >" +
                                         "<INCLUDE>ReasonCodeText</INCLUDE >" +
                                     "</QUERY>" +
                                  "</REQUEST>";

            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                List<Trainmessage> messages = JsonMessagesExtractor(result);
                return messages;
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
            var jsonStation = JsonConvert.DeserializeObject<StationRootobject>(result);
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
        private List<Trainannouncement> JsonDepartureExtractor(string result)
        {
            var jsonDeparture = JsonConvert.DeserializeObject<Rootobject>(result);
            var departureList = jsonDeparture.RESPONSE.RESULT;
            List<Trainannouncement> departureTrainList = new();
            //List<Trainannouncement> departureTrainList = departureList.Where();

            foreach (var departures in departureList)
            {
                foreach (var departure in departures.TrainAnnouncement)
                {
                    departureTrainList.Add(departure);
                }
            }
            return departureTrainList;
        }
        private TrainstationName JsonNameExtractor(string result)
        {
            var jsonName = JsonConvert.DeserializeObject<NameRootobject>(result);
            var jsonResult = jsonName.RESPONSE.RESULT;
            var destinationResult = jsonResult[0];
            var destination = destinationResult.TrainStation[0];

            return destination;
        }

        private List<Trainmessage> JsonMessagesExtractor(string result)
        {
            var jsonMessage = JsonConvert.DeserializeObject<MessagesRootobject>(result);
            var jsonResult = jsonMessage.RESPONSE.RESULT;
            List<Trainmessage> messages = new();

            foreach (var results in jsonResult)
            {
                foreach (var message in results.TrainMessage)
                {
                    messages.Add(message);
                }
            }
            return messages;
        }
    }
}
