namespace OkyanusServis.Api.Dtos;

/// <summary>Belge bilgisi (içerik hariç) — liste ve kart için.</summary>
public class DocumentDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}
