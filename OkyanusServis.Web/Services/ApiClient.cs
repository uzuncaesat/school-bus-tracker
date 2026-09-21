using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using OkyanusServis.Web.Models;

namespace OkyanusServis.Web.Services;

/// <summary>
/// Backend API çağrılarını tek yerde toplayan istemci.
/// HttpClient DI'dan gelir; adresi Program.cs'te API'ye ayarlandı.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    // GET /api/registrations  -> kayıt listesi
    public async Task<List<RegistrationDto>> GetRegistrationsAsync()
    {
        // JSON'u otomatik List<RegistrationDto>'ya çevirir. Boşsa boş liste döndür.
        var list = await _http.GetFromJsonAsync<List<RegistrationDto>>("api/registrations");
        return list ?? new List<RegistrationDto>();
    }

    // GET /api/registrations/5 -> tek kayıt
    public async Task<RegistrationDto?> GetRegistrationAsync(int id)
        => await _http.GetFromJsonAsync<RegistrationDto>($"api/registrations/{id}");

    // GET /api/stats -> panel özeti
    public async Task<StatsDto?> GetStatsAsync()
        => await _http.GetFromJsonAsync<StatsDto>("api/stats");

    // ---- Personel hesapları ----
    public async Task<List<StaffAccountDto>> GetStaffAsync()
        => await _http.GetFromJsonAsync<List<StaffAccountDto>>("api/staff") ?? new();

    public async Task<string?> CreateStaffAsync(string email, string password, string role)
    {
        var resp = await _http.PostAsJsonAsync("api/staff", new { email, password, role });
        if (resp.IsSuccessStatusCode) return null;
        var m = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(m) ? "Oluşturulamadı." : m;
    }

    public async Task<string?> ResetStaffPasswordAsync(string userId, string password)
    {
        var resp = await _http.PostAsJsonAsync($"api/staff/{userId}/reset-password", new { password });
        if (resp.IsSuccessStatusCode) return null;
        var m = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(m) ? "Sıfırlanamadı." : m;
    }

    public async Task<string?> DeleteStaffAsync(string userId)
    {
        var resp = await _http.DeleteAsync($"api/staff/{userId}");
        if (resp.IsSuccessStatusCode) return null;
        var m = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(m) ? "Silinemedi." : m;
    }

    // GET /api/registrations/mine -> giriş yapan velinin kendi kayıtları
    public async Task<List<RegistrationDto>> GetMineAsync()
        => await _http.GetFromJsonAsync<List<RegistrationDto>>("api/registrations/mine") ?? new();

    // GET /api/parent-accounts -> tüm veli hesapları
    public async Task<List<ParentAccountDto>> GetParentAccountsAsync()
        => await _http.GetFromJsonAsync<List<ParentAccountDto>>("api/parent-accounts") ?? new();

    // POST /api/parent-accounts/{userId}/reset-password -> şifre sıfırla
    public async Task<string?> ResetParentPasswordAsync(string userId, string password)
    {
        var resp = await _http.PostAsJsonAsync($"api/parent-accounts/{userId}/reset-password", new { password });
        if (resp.IsSuccessStatusCode) return null;
        var msg = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(msg) ? "Sıfırlanamadı." : msg;
    }

    // POST /api/registrations/5/parent-account -> kayda veli girişi tanımla (Admin)
    public async Task<string?> CreateParentAccountAsync(int registrationId, string email, string? password)
    {
        var resp = await _http.PostAsJsonAsync($"api/registrations/{registrationId}/parent-account",
            new { email, password });
        if (resp.IsSuccessStatusCode) return null;
        var msg = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(msg) ? "Hesap oluşturulamadı." : msg;
    }

    // POST /api/registrations -> yeni kayıt (formda kullanacağız)
    public async Task<RegistrationDto?> CreateRegistrationAsync(RegistrationCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/registrations", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegistrationDto>();
    }

    // PUT /api/registrations/5 -> güncelle (araç atama, ödeme/durum)
    public async Task<RegistrationDto?> UpdateRegistrationAsync(int id, RegistrationUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/registrations/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegistrationDto>();
    }

    // PUT /api/registrations/5/details -> temel bilgileri düzenle
    public async Task<RegistrationDto?> UpdateRegistrationDetailsAsync(int id, RegistrationEditDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/registrations/{id}/details", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegistrationDto>();
    }

    // DELETE /api/registrations/5 -> kaydı sil (Admin)
    public async Task DeleteRegistrationAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/registrations/{id}");
        response.EnsureSuccessStatusCode();
    }

    // GET /api/vehicles -> araç listesi
    public async Task<List<VehicleDto>> GetVehiclesAsync()
        => await _http.GetFromJsonAsync<List<VehicleDto>>("api/vehicles") ?? new();

    // POST /api/vehicles -> yeni araç (sadece Admin)
    public async Task<VehicleDto?> CreateVehicleAsync(VehicleCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/vehicles", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<VehicleDto>();
    }

    // PUT /api/vehicles/5 -> araç güncelle (cihaz no dahil, Admin)
    public async Task UpdateVehicleAsync(int id, VehicleUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/vehicles/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    // POST /api/vehicles/import-arvento -> Arvento'dan araçları içe aktar (Admin)
    public record ArventoImportResult(int Created, int Updated, int Total, string? Message);
    public async Task<ArventoImportResult?> ImportArventoVehiclesAsync()
    {
        var response = await _http.PostAsync("api/vehicles/import-arvento", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArventoImportResult>();
    }

    // DELETE /api/vehicles/5 -> araç sil (sadece Admin)
    public async Task DeleteVehicleAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/vehicles/{id}");
        response.EnsureSuccessStatusCode();
    }

    // ---- Takip / yaklaşım ayarları (Admin) ----
    public async Task<ServiceConfigDto?> GetConfigAsync()
        => await _http.GetFromJsonAsync<ServiceConfigDto>("api/config");

    public async Task UpdateConfigAsync(ServiceConfigDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/config", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> TestArventoAsync()
    {
        var response = await _http.PostAsync("api/config/test-arvento", null);
        return await response.Content.ReadAsStringAsync();
    }

    // ---- Güzergah sırası ----
    public async Task SetRouteOrderAsync(int vehicleId, List<int> orderedIds)
    {
        var resp = await _http.PostAsJsonAsync("api/registrations/route-order",
            new { vehicleId, orderedIds });
        resp.EnsureSuccessStatusCode();
    }

    // ---- Kardeş / aile ----
    public async Task<List<SiblingBriefDto>> GetSiblingsAsync(int id)
        => await _http.GetFromJsonAsync<List<SiblingBriefDto>>($"api/registrations/{id}/siblings") ?? new();

    public async Task LinkSiblingAsync(int id, int targetId)
    {
        var resp = await _http.PostAsync($"api/registrations/{id}/sibling/{targetId}", null);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UnlinkSiblingAsync(int id)
    {
        var resp = await _http.DeleteAsync($"api/registrations/{id}/sibling");
        resp.EnsureSuccessStatusCode();
    }

    // ---- Canlı takip (velinin aracı) ----
    public async Task<TrackingDto?> GetTrackingAsync()
        => await _http.GetFromJsonAsync<TrackingDto>("api/tracking/mine");

    // ---- Velinin kendi bildirimleri ----
    public async Task<List<NotificationDto>> GetMyNotificationsAsync()
        => await _http.GetFromJsonAsync<List<NotificationDto>>("api/notifications/mine") ?? new();

    public async Task MarkMyNotificationsReadAsync()
    {
        var response = await _http.PostAsync("api/notifications/mark-read", null);
        response.EnsureSuccessStatusCode();
    }

    // GET /api/notifications?registrationId=5 -> o kaydın bildirimleri
    public async Task<List<NotificationDto>> GetNotificationsAsync(int registrationId)
        => await _http.GetFromJsonAsync<List<NotificationDto>>($"api/notifications?registrationId={registrationId}") ?? new();

    // POST /api/notifications -> yeni bildirim (bindi/indi)
    public async Task CreateNotificationAsync(NotificationCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/notifications", dto);
        response.EnsureSuccessStatusCode();
    }

    // GET /api/announcements -> duyurular (en yeni önce)
    public async Task<List<AnnouncementDto>> GetAnnouncementsAsync()
        => await _http.GetFromJsonAsync<List<AnnouncementDto>>("api/announcements") ?? new();

    // POST /api/announcements -> yeni duyuru (sadece Admin)
    public async Task CreateAnnouncementAsync(AnnouncementCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/announcements", dto);
        response.EnsureSuccessStatusCode();
    }

    // ---- Belgeler (sözleşme vb.) ----

    // GET /api/registrations/5/documents -> kaydın belgeleri
    public async Task<List<DocumentDto>> GetDocumentsAsync(int registrationId)
        => await _http.GetFromJsonAsync<List<DocumentDto>>($"api/registrations/{registrationId}/documents") ?? new();

    // POST /api/registrations/5/documents -> belge yükle
    public async Task UploadDocumentAsync(int registrationId, IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10 MB
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _http.PostAsync($"api/registrations/{registrationId}/documents", content);
        response.EnsureSuccessStatusCode();
    }

    // GET /api/documents/5 -> belge içeriği (byte)
    public async Task<byte[]> DownloadDocumentAsync(int id)
        => await _http.GetByteArrayAsync($"api/documents/{id}");

    // DELETE /api/documents/5 -> belge sil (Admin)
    public async Task DeleteDocumentAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/documents/{id}");
        response.EnsureSuccessStatusCode();
    }
}
