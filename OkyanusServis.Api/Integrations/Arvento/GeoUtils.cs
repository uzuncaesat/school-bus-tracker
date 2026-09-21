namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>Coğrafi hesaplamalar. İki nokta arası kuş uçuşu mesafe vb.</summary>
public static class GeoUtils
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>İki koordinat arası kuş uçuşu mesafe (metre) — haversine.</summary>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Bir hedef noktadan verilen mesafe (metre) ve yön (derece) kadar
    /// uzakta yeni bir koordinat üretir. Mock'ta aracı "eve yaklaşıyor" gibi
    /// konumlandırmak için kullanılır.
    /// </summary>
    public static (double Lat, double Lon) Offset(double lat, double lon, double meters, double bearingDeg)
    {
        double angular = meters / EarthRadiusMeters;
        double bearing = ToRad(bearingDeg);
        double latR = ToRad(lat);
        double lonR = ToRad(lon);

        double lat2 = Math.Asin(Math.Sin(latR) * Math.Cos(angular) +
                                Math.Cos(latR) * Math.Sin(angular) * Math.Cos(bearing));
        double lon2 = lonR + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular) * Math.Cos(latR),
                                        Math.Cos(angular) - Math.Sin(latR) * Math.Sin(lat2));
        return (ToDeg(lat2), ToDeg(lon2));
    }

    private static double ToRad(double d) => d * Math.PI / 180.0;
    private static double ToDeg(double r) => r * 180.0 / Math.PI;
}
