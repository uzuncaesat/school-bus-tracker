namespace OkyanusServis.Api.Entities;

/// <summary>
/// Bir kayda (veliye) ait yüklenmiş belge — imzalı servis sözleşmesi vb.
/// Dosya içeriği veritabanında (varbinary) saklanır.
/// </summary>
public class RegistrationDocument
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }             // hangi kayda ait
    public Registration? Registration { get; set; }

    public string FileName { get; set; } = string.Empty;   // orijinal dosya adı
    public string ContentType { get; set; } = string.Empty; // application/pdf vb.
    public long Size { get; set; }                          // byte cinsinden boyut
    public byte[] Data { get; set; } = Array.Empty<byte>(); // dosya içeriği
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
