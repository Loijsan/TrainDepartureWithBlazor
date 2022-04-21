using ApiConnections.Data;
using static ApiConnections.Models.JsonDepartureModel;
using static ApiConnections.Models.JsonStationObject;


// Antingen kan man söka på ett fast ord...
//string station = "Ytter";

// Eller så får man använda sökord:


Console.WriteLine("Hello there, please enter the name of the station you wish to travel form: ");
string station = Console.ReadLine();

//TrainDepartures trainDepatures = new();
//List<string> stationList = await trainDepatures.FindStationsAsync(station);

//TrainDepartureHttp http = new();
//List<string> stationList = await http.CallTransportApiAsync(station);

ApiConnection departures = new();
List<Trainstation> stationList = await departures.GetStationsAsync(station);

foreach (var stop in stationList)
{
    Console.WriteLine($"Found {stop.AdvertisedLocationName}, short {stop.LocationSignature}");
}
Console.WriteLine("To find the departures, please enter the short name for the station: ");
string shortName = Console.ReadLine();
List<Trainannouncement> departureList = await departures.GetDeparturesAsync(shortName);

foreach (var destination in departureList)
{
    if (destination.ToLocation is not null)
    {
        var place = destination.ToLocation[0];
        var placename = await departures.GetStationNameAsync(place.LocationName.ToString());
        Console.WriteLine($"Train to {placename.AdvertisedLocationName} leaves at {destination.AdvertisedTimeAtLocation.TimeOfDay} from track {destination.TrackAtLocation}");
    }
}

//ExampleCode.PresentInfo(); // Denna är exempelkoden från Trafikverket
Console.WriteLine("Press the any key to continue");
Console.ReadKey();
