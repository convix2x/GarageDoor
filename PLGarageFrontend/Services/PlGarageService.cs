using System.Xml.Linq;
using PLGarageFrontend.Models;

namespace PLGarageFrontend.Services;

public class PLGarageService(HttpClient http)
{
    public string BaseUrl { get; set; } = "http://karting.playbredum.ru";

    public async Task<CreationsPage> GetTracksAsync(
        int page = 1,
        int perPage = 20,
        string sortColumn = "created_at",
        string sortOrder = "desc",
        string? keyword = null,
        bool isMnr = false)
    {
        // LBPK uses /tracks.xml, MNR uses /player_creations/search.xml.
        // This is really weird decision, though its probably not jacks decision,
        // but UFGs. Thanks, UFG!
        string endpoint = isMnr
            ? $"{BaseUrl}/player_creations/search.xml?filters[player_creation_type]=TRACK"
            : $"{BaseUrl}/tracks.xml?";

        var url = isMnr
            ? $"{BaseUrl}/player_creations/search.xml" +
              $"?page={page}" +
              $"&per_page={perPage}" +
              $"&platform=PS3" +
              $"&sort_column={sortColumn}" +
              $"&sort_order={sortOrder}" +
              $"&filters[player_creation_type]=TRACK"
            : $"{BaseUrl}/tracks.xml" +
              $"?page={page}" +
              $"&per_page={perPage}" +
              $"&platform=PS3" +
              $"&sort_column={sortColumn}" +
              $"&sort_order={sortOrder}";

        if (!string.IsNullOrWhiteSpace(keyword))
            url += $"&keyword={Uri.EscapeDataString(keyword)}";

        try
        {
            var xml = await http.GetStringAsync(url);
            return ParseCreationsPage(xml);
        }
        catch
        {
            return new CreationsPage();
        }
    }

    private static CreationsPage ParseCreationsPage(string xml)
    {
        var doc = XDocument.Parse(xml);

        var pageEl = doc.Descendants("player_creations").FirstOrDefault();
        var result = new CreationsPage
        {
            Total = (int?)pageEl?.Attribute("total") ?? 0,
            TotalPages = (int?)pageEl?.Attribute("total_pages") ?? 0,
            Page = (int?)pageEl?.Attribute("page") ?? 1,
        };

        result.Tracks = doc.Descendants("player_creation")
            .Select(x => new TrackCreation
            {
                Id = (int?)x.Attribute("id") ?? 0,
                Name = (string?)x.Attribute("name") ?? "Unnamed Track",
                Username = (string?)x.Attribute("username") ?? "Unknown",
                Description = (string?)x.Attribute("description") ?? "",
                Downloads = (int?)x.Attribute("downloads") ?? 0,
                Hearts = (int?)x.Attribute("hearts") ?? 0,
                RatingUp = (int?)x.Attribute("rating_up") ?? 0,
                RatingDown = (int?)x.Attribute("rating_down") ?? 0,
                Views = (int?)x.Attribute("views") ?? 0,
                RacesStarted = (int?)x.Attribute("races_started") ?? 0,
                NumLaps = (int?)x.Attribute("num_laps") ?? 0,
                NumRacers = (int?)x.Attribute("num_racers") ?? 0,
                RaceType = (string?)x.Attribute("race_type") ?? "",
                Difficulty = (string?)x.Attribute("difficulty") ?? "",
                Tags = (string?)x.Attribute("tags") ?? "",
                IsTeamPick = (bool?)x.Attribute("is_team_pick") ?? false,
                IsRemixable = (bool?)x.Attribute("is_remixable") ?? false,
                FirstPublished = (string?)x.Attribute("first_published") ?? "",
                UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                Platform = (string?)x.Attribute("platform") ?? "",
            })
            .ToList();

        return result;
    }

    public async Task<string> GetInstanceNameAsync()
    {
        try { return await http.GetStringAsync($"{BaseUrl}/api/GetInstanceName"); }
        catch { return "PL Garage"; }
    }

    public async Task<int> GetPlayerCountAsync()
    {
        try { return int.Parse(await http.GetStringAsync($"{BaseUrl}/api/playercounts/sessioncount")); }
        catch { return 0; }
    }
}