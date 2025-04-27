using ApiConnections.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using static ApiConnections.Models.DepartureModel;
using static ApiConnections.Models.JsonStationNameModel;
using static ApiConnections.Models.JsonStationObject;
using static ApiConnections.Models.JsonTrainInfoModel;
using static ApiConnections.Models.StationMessages;

namespace ApiConnections.Data
{
    public class ApiConnection
    {
        private readonly string _token;

        public ApiConnection(IConfiguration configuration)
        {
            // Yay for them internets!
            _token = configuration.GetSection("TrainToken").Value;
        }
        public async Task<List<Trainstation>> GetStationsAsync(string stationName)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{_token}'/>" +
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
        public async Task<List<TrainDepartureModel>> GetDeparturesAsync(string locationSignature)
        {
            // TODO! - Hade hellre begränsat den i tid än i antal...
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{_token}' />" +
                                    "<QUERY objecttype='TrainAnnouncement' schemaversion='1.3' orderby='AdvertisedTimeAtLocation'>" +
                                    "<FILTER>" +
                                    "<AND>" +
                                    "<EQ name='ActivityType' value='Avgang' />" +
                                    $"<EQ name='LocationSignature' value='{locationSignature}' />" +
                                    "<OR>" +
                                          "<AND>" +
                                                "<GT name='AdvertisedTimeAtLocation' value='$dateadd(-00:10:00)' />" +
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

            // This request is sent to the API
            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                // The response, if not null, is sent to the JsonExtractor
                List<Trainannouncement> departureList = JsonDepartureExtractor(result);
                List<TrainDepartureModel> trainDepartureList = new();

                // The call to get the destination names is made here.
                foreach (var departure in departureList)
                {
                    if (departure.ToLocation is not null)
                    {
                        var place = departure.ToLocation[0];
                        var stationName = await GetStationNameAsync(place.LocationName);
                        var info = await GetTrainInfoAsync(departure.AdvertisedTrainIdent);

                        TrainDepartureModel trainDeparture = new()
                        {
                            Announcements = departure,
                            LocationFullName = stationName.AdvertisedLocationName.ToString(),
                            ProductInformation = info
                        };
                        trainDepartureList.Add(trainDeparture);
                    }
                }
                return trainDepartureList;
            }
            return null;
        }
        public async Task<TrainstationName> GetStationNameAsync(string locationSignature)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{_token}'/>" +
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
                return stationName;
            }
            return null;
        }
        public async Task<List<Trainmessage>> GetStationMessagesAsync(string locationSignature)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{_token}'/>" +
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
        public async Task<string> GetTrainInfoAsync(string trainNumber)
        {
            string requestBody = "<REQUEST>" +
                                     $"<LOGIN authenticationkey='{_token}'/>" +
                                     "<QUERY objecttype='TrainAnnouncement' schemaversion='1.3'>" +
                                         "<FILTER>" +
                                             $"<EQ name='AdvertisedTrainIdent' value='{trainNumber}' />" +
                                         "</FILTER>" +
                                         "<INCLUDE>InformationOwner</INCLUDE >" +
                                         "<INCLUDE>ProductInformation</INCLUDE>" +
                                     "</QUERY>" +
                                  "</REQUEST>";

            var result = await ApiPostCaller(requestBody);

            if (result is not null)
            {
                var resultList = JsonTrainInfoExtractor(result);

                // Why do APIs change? But this is how I learn stuff...
                if (resultList is not null)
                {
                    if (resultList.ProductInformation is not null)
                    {
                        var info = resultList.ProductInformation[0];
                        return info;
                    }
                    if (resultList.InformationOwner is not null)
                    {
                        var info = resultList.InformationOwner;
                        return info;
                    }
                    
                }
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
                foreach (var station in stations.TrainStation)
                {
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
        private InfoTrainannouncement JsonTrainInfoExtractor(string result)
        {
            var jsonName = JsonConvert.DeserializeObject<InfoRootobject>(result);
            var jsonResult = jsonName.RESPONSE.RESULT;
            var informationResult = jsonResult[0];
            if (informationResult != null)
            {
                return null;
            }
            var information = informationResult.TrainAnnouncement[0];

            return information;
        }
    }
}
