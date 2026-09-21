namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Bir servis aracının anlık konumu. Arvento'dan (ya da mock'tan) dönen tek satır.
/// </summary>
public record VehiclePosition(
    string Node,        // Arvento cihaz no
    double Latitude,
    double Longitude,
    double Speed,       // km/s
    DateTime Timestamp  // konumun alındığı an (UTC)
);
