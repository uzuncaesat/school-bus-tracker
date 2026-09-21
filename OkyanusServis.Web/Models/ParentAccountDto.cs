namespace OkyanusServis.Web.Models;

public class ParentAccountDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Students { get; set; } = new();
}
