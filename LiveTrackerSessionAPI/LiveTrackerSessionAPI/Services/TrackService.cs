using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using LiveTrackerSessionAPI.Hubs;
using LiveTrackerSessionAPI.Repository;
using LiveTrackerSessionAPI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SessionAPICommonModels;
using SessionAPICommonModels.Dtos;
using SessionAPICommonModels.ViewModels;
using SessionAPIModels;
using LineString = GeoJSON.Net.Geometry.LineString;
using Position = GeoJSON.Net.Geometry.Position;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;
using System.Diagnostics;


namespace LiveTrackerSessionAPI.Services;

public class TrackService : ITrackService
{

    #region Declaration

    private readonly IHubContext<RaceHub> _raceHubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository _repository;

    protected static readonly ConcurrentDictionary<int, RaceData> RaceData = new();

    public TrackService(IHubContext<RaceHub> raceHubContext, IHttpContextAccessor httpContextAccessor, IRepository repository)
    {
        _raceHubContext = raceHubContext;
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
    }

    #endregion

    #region Public Methods

    public void LoadRaceData(RaceDataDto raceDataDto)
    {
        var raceId = raceDataDto.RaceId;

        if(RaceData.ContainsKey(raceId))
            return;

        var sectors = DivideTrackIntoSectors(raceDataDto.GeoJsonData);
        var drivers = new ConcurrentDictionary<string, Driver>();

        foreach (var driverDto in raceDataDto.Drivers)
        {
            drivers[driverDto.Id.ToString()] = new Driver(driverDto.Id);
        }

        var raceData = new RaceData
        {
            RaceId = raceId,
            Sectors = sectors.Sectors,
            Drivers = drivers,
            LapCount = raceDataDto.LapCount,
            SectorAfterFinish = sectors.SectorAfterFinish
        };

        RaceData[raceId] = raceData;
    }

    public async Task UpdateGpsData(IList<GpsData> gpsData)
    {
        var start = DateTime.Now;

        var raceId = GetRaceIdFromToken();
        var race = RaceData[raceId];

        lock (race)
        {
            foreach (var data in gpsData)
            {
                UpdateDriverPosition(race, data.DriverId, data.Latitude, data.Longitude);
            }
        }

        var driverPositions = DetermineDriversOrder(race);
        var driversData = driverPositions.Select(driver => new DriverVM
        {
            DriverId = driver.Key,
            Latitude = driver.Value.Position?.Latitude ?? 0,
            Longitude = driver.Value.Position?.Longitude ?? 0,
            Finished = driver.Value.Finished
        }).ToList();
        var currentLap = GetCurrentLap(race, driverPositions);


        var raceData = new RaceDataVM
        {
            DriversData = driversData,
            LapCount = currentLap
        };
        var raceDataJson = JsonConvert.SerializeObject(raceData, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        await _repository.SaveData(raceId, raceDataJson);
        await _raceHubContext.Clients.Group(raceId.ToString()).SendAsync("ReceiveRaceData", raceDataJson);

        var end = DateTime.Now;
        var a = end - start;
        var b = "";
    }

    //only for testing
    public Tuple<List<GpsData>, int> GetRaceState(int raceId)
    {
        var start = DateTime.Now;
        var race = RaceData[raceId];

        var drivers = DetermineDriversOrder(race);
        var driversOutput = drivers.Select(driver => new GpsData
        {
            DriverId = driver.Key,
            Latitude = driver.Value.Position?.Latitude ?? 0,
            Longitude = driver.Value.Position?.Longitude ?? 0,
        }).ToList();

        var end = DateTime.Now;
        var delta = start - end;

        var currentLap = GetCurrentLap(race, drivers);

        return Tuple.Create(driversOutput, currentLap);
    }

    public IEnumerable<string>? UnloadRaceData(int raceId)
    {
        if (!RaceData.ContainsKey(raceId))
            return null;

        var race = RaceData[raceId];
        var result = DetermineDriversOrder(race).Select(x => x.Key);

        RaceData.Remove(raceId, out _);

        return result;
    }

    #endregion

    #region Private Methods

    private int GetRaceIdFromToken()
    {
        if (_httpContextAccessor is not { HttpContext: { } })
            throw new Exception("No http context");

        var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            throw new UnauthorizedAccessException();
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var tokenHandler = new JwtSecurityTokenHandler();

        if (tokenHandler.ReadToken(token) is not JwtSecurityToken jsonToken)
        {
            throw new UnauthorizedAccessException();
        }

        var raceIdString = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (raceIdString == null || !int.TryParse(raceIdString, out var raceId))
        {
            throw new UnauthorizedAccessException();
        }
        return raceId;
    }

    private static (IList<(LineString Line, Polygon BoundingBox)> Sectors, (LineString Line, Polygon BoundingBox)? SectorAfterFinish) DivideTrackIntoSectors(string geoJsonData)
    {
        var sectors = new List<(LineString Line, Polygon BoundingBox)>();

        var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJsonData);
        if (featureCollection is null)
        {
            throw new Exception("feature collection is null");
        }

        foreach (var feature in featureCollection.Features)
        {
            if (feature.Geometry is not LineString lineString)
                continue;

            var coordinates = lineString.Coordinates;

            for (var i = 0; i < coordinates.Count - 1; i++)
            {
                var currentCoordinate = coordinates[i];
                var nextCoordinate = coordinates[i + 1];

                var segmentPoints = new List<IPosition>
                {
                    currentCoordinate,
                    nextCoordinate
                };
                var segmentLineString = new LineString(segmentPoints);
                var boundingBox = CreateBoundingBox(segmentLineString);

                sectors.Add((segmentLineString, boundingBox));
            } 
        }

        (LineString Line, Polygon BoundingBox)? sectorAfterFinish = null;

        var firstPosition = sectors.First().Line.Coordinates.First();
        var lastPosition = sectors.Last().Line.Coordinates.Last();
        const double isCircuitTolerance = 0.001;

        //track is circuit
        if (Math.Abs(firstPosition.Latitude - lastPosition.Latitude) < isCircuitTolerance && Math.Abs(firstPosition.Longitude - lastPosition.Longitude) < isCircuitTolerance) 
            return (sectors, sectorAfterFinish);

        var sectorLineAfterFinish = GetSectorAfterFinish(sectors.Last().Line, 0.007);
        var boundingBoxAfterFinish = CreateBoundingBox(sectorLineAfterFinish, 0.0005);
        sectorAfterFinish = (sectorLineAfterFinish, boundingBoxAfterFinish);

        return (sectors, sectorAfterFinish);
    }

    public static LineString GetSectorAfterFinish(LineString lastSector, double extensionDistance)
    {
        
        var lastSegment = lastSector.Coordinates;
        var pointCount = lastSegment.Count;
        var point1 = lastSegment[pointCount - 2];
        var point2 = lastSegment[pointCount - 1];

        var direction = CalculateDirection(point1, point2);

        var extendedSector = ExtendLine(point2, direction, extensionDistance);

        return extendedSector;
    }

    /// <summary>
    /// Calculates angle of line between two points with x axis in radians
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    private static double CalculateDirection(IPosition point1, IPosition point2)
    {
        var deltaX = point2.Longitude - point1.Longitude;
        var deltaY = point2.Latitude - point1.Latitude;
        return Math.Atan2(deltaY, deltaX);
    }

    /// <summary>
    /// Creates line from startPoint of distance in direction 
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="direction">angle in radians</param>
    /// <param name="distance">distance in degrees</param>
    /// <returns></returns>
    private static LineString ExtendLine(IPosition startPoint, double direction, double distance)
    {
        // Calculate the new endpoint based on the direction and distance
        var newLongitude = startPoint.Longitude + Math.Cos(direction) * distance;
        var newLatitude = startPoint.Latitude + Math.Sin(direction) * distance;

        // Create the extended line
        var extendedLine = new LineString(new List<IPosition> { startPoint, new Position(newLatitude, newLongitude) });

        return extendedLine;
    }

    /// <summary>
    /// Creates rectangle of width around the line
    /// </summary>
    /// <param name="lineString"></param>
    /// <param name="boundingBoxWidth"></param>
    /// <returns></returns>
    private static Polygon CreateBoundingBox(LineString lineString, double boundingBoxWidth = 0.0002)
    {
        // Calculate direction vector of the line segment
        var directionX = lineString.Coordinates[1].Longitude - lineString.Coordinates[0].Longitude;
        var directionY = lineString.Coordinates[1].Latitude - lineString.Coordinates[0].Latitude;

        // Normalize the direction vector
        var length = Math.Sqrt(directionX * directionX + directionY * directionY);
        directionX /= length;
        directionY /= length;

        // Calculate perpendicular vector
        var perpendicularX = -directionY;
        var perpendicularY = directionX;

        var delta = boundingBoxWidth / 2;

        // Calculate points for bounding box
        var bottomRight = new Coordinate(lineString.Coordinates[0].Longitude + delta * perpendicularX, lineString.Coordinates[0].Latitude + delta * perpendicularY);
        var topRight = new Coordinate(lineString.Coordinates[1].Longitude + delta * perpendicularX, lineString.Coordinates[1].Latitude + delta * perpendicularY);
        var bottomLeft = new Coordinate(lineString.Coordinates[0].Longitude - delta * perpendicularX, lineString.Coordinates[0].Latitude - delta * perpendicularY);
        var topLeft = new Coordinate(lineString.Coordinates[1].Longitude - delta * perpendicularX, lineString.Coordinates[1].Latitude - delta * perpendicularY);

        var boundingBox = new Polygon(new LinearRing(new[] { topRight, bottomRight, bottomLeft, topLeft, topRight }));
        
        return boundingBox;
    }

    private static IEnumerable<KeyValuePair<string, Driver>> DetermineDriversOrder(RaceData race)
    {

        IList<KeyValuePair<string, Driver>> recentlyFinishedDrivers = new List<KeyValuePair<string, Driver>>();

        if (!race.IsCircuit && race.CurrentMaxSector > race.Sectors.Count / 4 * 3)
        {
            recentlyFinishedDrivers = race.Drivers.Where(x => x.Value is { SectorId: int.MaxValue, FinalPosition: 0 } )
                .OrderByDescending(driver => CalculateDistance(race.SectorAfterFinish!.Value.Line.Coordinates[0], GetOrthogonalProjectionOfPositionOnLine(race.SectorAfterFinish!.Value.Line, driver.Value.Position!)))
                .ToList();

            var finishedDriversCount = race.Drivers.Count(driver => driver.Value.Finished);

            for (var i = 0; i < recentlyFinishedDrivers.Count; i++)
            {
                var driver = recentlyFinishedDrivers[i];
                if (driver.Value.FinalPosition == 0)
                {
                    driver.Value.FinalPosition = finishedDriversCount + i + 1;
                }
            }

        }
        else if (race.IsCircuit)
        {
            recentlyFinishedDrivers = race.Drivers.Where(x => x.Value.LapCount > race.LapCount)
                .OrderByDescending(driver => driver.Value.SectorId)
                .ThenByDescending(driver => CalculateDistance(race.Sectors[0].Line.Coordinates[0], GetOrthogonalProjectionOfPositionOnLine(race.Sectors[0].Line, driver.Value.Position!)))
                .ToList();

            var finishedDriversCount = race.Drivers.Count(driver => driver.Value.Finished);

            for (var i = 0; i < recentlyFinishedDrivers.Count; i++)
            {
                var driver = recentlyFinishedDrivers[i];
                if (driver.Value.FinalPosition == 0)
                {
                    driver.Value.FinalPosition = finishedDriversCount + i + 1;
                }
            }
        }

        var ret = race.Drivers.OrderBy(driver => driver.Value.FinalPosition == 0 ? int.MaxValue : driver.Value.FinalPosition)
            .ThenByDescending(driver => driver.Value.LapCount)
            .ThenByDescending(driver => driver.Value.SectorId)
            .ThenByDescending(driver =>
            {
                var driverSector = driver.Value.SectorId == int.MaxValue
                    ? race.SectorAfterFinish
                    : race.Sectors[driver.Value.SectorId];

                if (driver.Value.Position is null || !driverSector.HasValue)
                {
                    return double.MinValue; // Place them at the end of the sorted list
                }

                // Calculate distance from driver to the start point of the sector
                var driverProjectedPosition =
                    GetOrthogonalProjectionOfPositionOnLine(driverSector.Value.Line, driver.Value.Position);
                var distanceToSectorStart =
                    CalculateDistance(driverSector.Value.Line.Coordinates[0], driverProjectedPosition);
                return distanceToSectorStart;
            });

        return ret;
    }

    private static void UpdateDriverPosition(RaceData race, string driverId, double latitude, double longitude)
    {
        if (!race.Drivers.ContainsKey(driverId))
            return;

        var driver = race.Drivers[driverId];

        var previousPosition = driver.Position;
        var currentPosition = new GpsPoint(latitude, longitude);

        var previousSector = driver.SectorId;
        var currentSector = GetDriverSector(currentPosition, previousSector, race.Sectors, race.SectorAfterFinish);

        var sectorCount = race.Sectors.Count;

        if (race.IsCircuit && previousPosition is null)
            driver.LapCount = 0;

        driver.Position = currentPosition;
        driver.SectorId = currentSector;

        if (currentSector > race.CurrentMaxSector)
        {
            race.CurrentMaxSector = currentSector;
        }

        if (race.IsCircuit && HasCrossedStartFinishLine(previousSector, currentSector, sectorCount))
        {
            driver.LapCount++;
        }
    }

    private static int GetDriverSector(Geometry position, int previousSector, IList<(LineString Line, Polygon BoundingBox)> sectors, (LineString Line, Polygon BoundingBox)? sectorAfterFinish)
    {
        //is circuit
        if (sectorAfterFinish is null)
        {
            var i = previousSector;
            do
            {
                if (IsInSector(sectors[i].BoundingBox, position))
                    return i;
                i = (i + 1) % sectors.Count;
            } while (i != previousSector);
        }
        else
        {
            for (var i = previousSector; i < sectors.Count; i++)
            {
                if(IsInSector(sectors[i].BoundingBox, position))
                    return i;
            }

            //finished
            if (IsInSector(sectorAfterFinish.Value.BoundingBox, position))
                return int.MaxValue;
        }

        return previousSector;
    }

    private static bool IsInSector(Geometry boundingBox, Geometry point)
    {
        return boundingBox.Contains(point);
    }

    private static bool HasCrossedStartFinishLine(int previousSector, int currentSector, int sectorCount) =>
        previousSector > sectorCount - sectorCount / 4 && currentSector < sectorCount / 4;

    private int GetCurrentLap(RaceData race, IEnumerable<KeyValuePair<string, Driver>> driverPositions) =>
        race.IsCircuit ? driverPositions.Max(driver => driver.Value.LapCount) : 0;


    /// <summary>
    /// Calculates distance between two geographical points
    /// </summary>
    /// <param name="coord1"></param>
    /// <param name="coord2"></param>
    /// <returns></returns>
    private static double CalculateDistance(IPosition coord1, IPosition coord2)
    {
        const double earthRadius = 6371; // Earth radius in kilometers

        var lat1 = ToRadians(coord1.Latitude);
        var lon1 = ToRadians(coord1.Longitude);
        var lat2 = ToRadians(coord2.Latitude);
        var lon2 = ToRadians(coord2.Longitude);

        var dLon = lon2 - lon1;
        var dLat = lat2 - lat1;

        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = earthRadius * c;

        return distance * 1000; // Distance in meters
    }

    //private static double CalculateDistance(IPosition coord1, IPosition coord2)
    //{
    //    return Math.Sqrt(Math.Pow(coord2.Latitude - coord1.Latitude, 2) + Math.Pow(coord2.Longitude - coord1.Longitude, 2));
    //}

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private static IPosition GetOrthogonalProjectionOfPositionOnLine(LineString line, IPosition driverPos)
    {
        // AB are point of line
        var A = new Point(line.Coordinates[0].Longitude, line.Coordinates[0].Latitude);
        var B = new Point(line.Coordinates[1].Longitude, line.Coordinates[1].Latitude);
        // C is point of driver
        var C = new Point(driverPos.Longitude, driverPos.Latitude);

        //Line AB represented as a1x + b1y = c1
        var a1 = B.Y - A.Y;
        var b1 = A.X - B.X;
        var c1 = a1 * A.X + b1 * A.Y;

        //Line CD is orthogonal to AB
        //Line CD represented as a2x + b2y = c2
        var a2 = b1;
        var b2 = -a1;
        var c2 = a2 * C.X + b2 * C.Y;

        var determinant = a1 * b2 - a2 * b1;

        var x = (b2 * c1 - b1 * c2) / determinant;
        var y = (a1 * c2 - a2 * c1) / determinant;

        return new Position(y, x);
    }

    #endregion
}
