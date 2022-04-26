using ApiConnections.Data;
using static ApiConnections.Models.DepartureModel;
using static ApiConnections.Models.JsonDepartureModel;
using static ApiConnections.Models.JsonStationObject;
using static ApiConnections.Models.StationMessages;

// Get the destination from the user
Console.WriteLine("Hello there, please enter the name of the station you wish to travel form: ");
string station = Console.ReadLine();
ApiConnection departures = new();
List<Trainstation> stationList = await departures.GetStationsAsync(station);
// List the station that matched the search word
foreach (var stop in stationList)
{
    Console.WriteLine($"Found {stop.AdvertisedLocationName}, short {stop.LocationSignature}");
}

// Get the locationName from the user
Console.WriteLine("To find the departures, please enter the short name for the station: ");
string shortName = Console.ReadLine();
List<Trainannouncement> departureList = await departures.GetDeparturesAsync(shortName);
List<Trainmessage> messages = await departures.GetStationMessagesAsync(shortName);
// List the messages for the chosen station
if (messages is not null)
{
    foreach (var message in messages)
    {
        Console.WriteLine($"Station message: {message.ExternalDescription}");
    }
}
// List the departures from the chosen station
foreach (var destination in departureList)
{
    if (destination.ToLocation is not null)
    {
        var place = destination.ToLocation[0];
        var placename = await departures.GetStationNameAsync(place.LocationName.ToString());
        Console.WriteLine($"Train to {placename.AdvertisedLocationName} leaves at {destination.AdvertisedTimeAtLocation} or {destination.EstimatedTimeAtLocation} from track {destination.TrackAtLocation}. Departure cancelled? {destination.Canceled.Value} and deviation? {destination.Deviation}");
    }
}

Console.WriteLine("Press the any key to continue");
Console.ReadKey();
