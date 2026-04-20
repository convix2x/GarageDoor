/* NOTE 
 * This service is all loosely written over the api
 * I will adjust as needed when I test the moderation panels, but for now this is just a best effort implementation based on the api spec reading
 * through pl garage
 * 
 * i'm not sane, help
 */

using System.Net;
using System.Net.Http.Headers;

namespace PLGarageFrontend.Services;

public class ModerationService
{
    private readonly HttpClient _http;
    private string? _token;

    public string BaseUrl { get; set; } = "https://karting.playbredum.ru";
    public bool IsLoggedIn => _token != null;

    public ModerationService(HttpClient http)
    {
        _http = http;
    }

    private HttpRequestMessage Req(HttpMethod method, string path)
    {
        var req = new HttpRequestMessage(method, $"{BaseUrl}{path}");
        if (_token != null)
            req.Headers.Add("Cookie", $"Token={_token}");
        return req;
    }

    private async Task<string?> Send(HttpRequestMessage req)
    {
        try
        {
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            return (await resp.Content.ReadAsStringAsync()).Trim();
        }
        catch { return null; }
    }

    private async Task<string?> Get(string path) =>
        await Send(Req(HttpMethod.Get, path));

    private async Task<string?> Post(string path) =>
        await Send(Req(HttpMethod.Post, path));

    private async Task<string?> Delete(string path) =>
        await Send(Req(HttpMethod.Delete, path));

    public async Task<bool> LoginAsync(string username, string password)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseUrl}/api/moderation/login" +
            $"?login={Uri.EscapeDataString(username)}" +
            $"&password={Uri.EscapeDataString(password)}"
        );

        try
        {
            var resp = await _http.SendAsync(req);
            var body = (await resp.Content.ReadAsStringAsync()).Trim();

            if (body != "ok") return false;

            if (resp.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    if (cookie.StartsWith("Token="))
                    {
                        _token = cookie.Split(';')[0]["Token=".Length..];
                        break;
                    }
                }
            }

            return true;
        }
        catch { return false; }
    }

    public void Logout() => _token = null;


    public Task<string?> GetPermissionsAsync() =>
        Get("/api/moderation/permissions");

    public Task<string?> SetUsernameAsync(string username) =>
        Post($"/api/moderation/set_username?username={Uri.EscapeDataString(username)}");

    public Task<string?> SetPasswordAsync(string password) =>
        Post($"/api/moderation/set_password?password={Uri.EscapeDataString(password)}");


    public Task<string?> GetGriefReportsAsync(int page, int perPage, string? context = null, int? from = null)
    {
        var q = $"/api/moderation/grief_reports?page={page}&per_page={perPage}";
        if (context != null) q += $"&context={Uri.EscapeDataString(context)}";
        if (from != null) q += $"&from={from}";
        return Get(q);
    }

    public Task<string?> GetGriefReportAsync(int id) =>
        Get($"/api/moderation/grief_reports/{id}");


    public Task<string?> SetModerationStatusAsync(int id, string status) =>
        Post($"/api/moderation/setStatus?id={id}&status={Uri.EscapeDataString(status)}");

    public Task<string?> GetPlayerCreationsWithStatusAsync(int page, int perPage, string status) =>
        Get($"/api/moderation/player_creations?page={page}&per_page={perPage}&status={Uri.EscapeDataString(status)}");

    public Task<string?> ResetCreationStatsAsync(int playerCreationId) =>
        Post($"/api/moderation/player_creations/{playerCreationId}/reset_stats");


    public Task<string?> GetUsersAsync(
        int page, int perPage,
        bool? playedMnr = null, bool? isPsnLinked = null,
        bool? isRpcnLinked = null, bool? isBanned = null)
    {
        var q = $"/api/moderation/users?page={page}&per_page={perPage}";
        if (playedMnr != null) q += $"&PlayedMNR={playedMnr.ToString()!.ToLower()}";
        if (isPsnLinked != null) q += $"&IsPSNLinked={isPsnLinked.ToString()!.ToLower()}";
        if (isRpcnLinked != null) q += $"&IsRPCNLinked={isRpcnLinked.ToString()!.ToLower()}";
        if (isBanned != null) q += $"&IsBanned={isBanned.ToString()!.ToLower()}";
        return Get(q);
    }

    public Task<string?> SetBanAsync(int id, bool isBanned) =>
        Post($"/api/moderation/setBan?id={id}&isBanned={isBanned.ToString().ToLower()}");

    public Task<string?> SetUserSettingsAsync(int id, bool showCreationsWithoutPreviews, bool allowOppositePlatform) =>
        Post($"/api/moderation/setUserSettings?id={id}" +
             $"&ShowCreationsWithoutPreviews={showCreationsWithoutPreviews.ToString().ToLower()}" +
             $"&AllowOppositePlatform={allowOppositePlatform.ToString().ToLower()}");

    public Task<string?> SetUserQuotaAsync(int id, int quota) =>
        Post($"/api/moderation/setUserQuota?id={id}&quota={quota}");

    public Task<string?> ResetUserProfileAsync(int id, bool removeCreations = false) =>
        Post($"/api/moderation/users/{id}/reset_profile?removeCreations={removeCreations.ToString().ToLower()}");


    public Task<string?> GetPlayerComplaintsAsync(int page, int perPage, int? from = null, int? playerId = null)
    {
        var q = $"/api/moderation/player_complaints?page={page}&per_page={perPage}";
        if (from != null) q += $"&from={from}";
        if (playerId != null) q += $"&playerID={playerId}";
        return Get(q);
    }

    public Task<string?> GetPlayerComplaintAsync(int id) =>
        Get($"/api/moderation/player_complaints/{id}");


    public Task<string?> GetPlayerCreationComplaintsAsync(int page, int perPage, int? from = null, int? playerId = null, int? playerCreationId = null)
    {
        var q = $"/api/moderation/player_creation_complaints?page={page}&per_page={perPage}";
        if (from != null) q += $"&from={from}";
        if (playerId != null) q += $"&playerID={playerId}";
        if (playerCreationId != null) q += $"&playerCreationID={playerCreationId}";
        return Get(q);
    }

    public Task<string?> GetPlayerCreationComplaintAsync(int id) =>
        Get($"/api/moderation/player_creation_complaints/{id}");


    public Task<string?> GetSystemEventsAsync(int page, int perPage) =>
        Get($"/api/moderation/system_events?page={page}&per_page={perPage}");

    public Task<string?> CreateSystemEventAsync(string topic, string description, string imageUrl) =>
        Post($"/api/moderation/system_events" +
             $"?topic={Uri.EscapeDataString(topic)}" +
             $"&description={Uri.EscapeDataString(description)}" +
             $"&imageURL={Uri.EscapeDataString(imageUrl)}");

    public Task<string?> EditSystemEventAsync(int id, string topic, string description, string imageUrl) =>
        Post($"/api/moderation/system_events/{id}" +
             $"?topic={Uri.EscapeDataString(topic)}" +
             $"&description={Uri.EscapeDataString(description)}" +
             $"&imageURL={Uri.EscapeDataString(imageUrl)}");

    public Task<string?> DeleteSystemEventAsync(int id) =>
        Delete($"/api/moderation/system_events/{id}");


    public Task<string?> GetAnnouncementsAsync(int page, int perPage, string? platform = null)
    {
        var q = $"/api/moderation/announcements?page={page}&per_page={perPage}";
        if (platform != null) q += $"&platform={Uri.EscapeDataString(platform)}";
        return Get(q);
    }

    public Task<string?> CreateAnnouncementAsync(string languageCode, string subject, string text, string platform) =>
        Post($"/api/moderation/announcements" +
             $"?languageCode={Uri.EscapeDataString(languageCode)}" +
             $"&subject={Uri.EscapeDataString(subject)}" +
             $"&text={Uri.EscapeDataString(text)}" +
             $"&platform={Uri.EscapeDataString(platform)}");

    public Task<string?> EditAnnouncementAsync(int id, string languageCode, string subject, string text, string platform) =>
        Post($"/api/moderation/announcements/{id}" +
             $"?languageCode={Uri.EscapeDataString(languageCode)}" +
             $"&subject={Uri.EscapeDataString(subject)}" +
             $"&text={Uri.EscapeDataString(text)}" +
             $"&platform={Uri.EscapeDataString(platform)}");

    public Task<string?> DeleteAnnouncementAsync(int id) =>
        Delete($"/api/moderation/announcements/{id}");


    public Task<string?> GetHotLapAsync() =>
        Get("/api/moderation/hotlap");

    public Task<string?> SetHotLapAsync(int creationId) =>
        Post($"/api/moderation/hotlap?creation={creationId}");

    public Task<string?> ResetHotLapAsync() =>
        Post("/api/moderation/hotlap/reset");

    public Task<string?> GetNextHotLapResetAsync() =>
        Get("/api/moderation/hotlap/until_next");

    public Task<string?> GetHotLapQueueAsync(int page, int perPage) =>
        Get($"/api/moderation/hotlap/queue?page={page}&per_page={perPage}");

    public Task<string?> AddToHotLapQueueAsync(int creationId) =>
        Post($"/api/moderation/hotlap/queue?creation={creationId}");

    public Task<string?> RemoveFromHotLapQueueAsync(int? index = null, int? creationId = null)
    {
        var q = "/api/moderation/hotlap/queue";
        if (index != null) q += $"?index={index}";
        if (creationId != null) q += (index != null ? "&" : "?") + $"creation={creationId}";
        return Delete(q);
    }


    public Task<string?> GetModeratorsAsync(int page, int perPage) =>
        Get($"/api/moderation/moderators?page={page}&per_page={perPage}");

    public Task<string?> GetModeratorAsync(int id) =>
        Get($"/api/moderation/moderators/{id}");

    public Task<string?> DeleteModeratorAsync(int id) =>
        Delete($"/api/moderation/moderators/{id}");

    public Task<string?> SetModeratorUsernameAsync(int id, string username) =>
        Post($"/api/moderation/{id}/set_username?username={Uri.EscapeDataString(username)}");

    public Task<string?> SetModeratorPasswordAsync(int id, string password) =>
        Post($"/api/moderation/{id}/set_password?password={Uri.EscapeDataString(password)}");
}