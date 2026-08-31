namespace BackEndWaterFloodApp.Helpers;

public static class GeoDistanceHelper
{
    private const double EarthRadiusKm = 6371.0;

    public static double HaversineDistanceKm(
        decimal latitude1,
        decimal longitude1,
        decimal latitude2,
        decimal longitude2
    )
    {
        var lat1 = DegreesToRadians((double)latitude1);
        var lat2 = DegreesToRadians((double)latitude2);
        var deltaLat = DegreesToRadians((double)(latitude2 - latitude1));
        var deltaLon = DegreesToRadians((double)(longitude2 - longitude1));

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
            + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
