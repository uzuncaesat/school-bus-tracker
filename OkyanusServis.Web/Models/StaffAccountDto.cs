namespace OkyanusServis.Web.Models;

public class StaffAccountDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // Admin | Driver
}
