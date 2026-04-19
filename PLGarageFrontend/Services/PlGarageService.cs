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
        bool isMnr = false,
        string platform = "PS3")
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
              $"&platform={platform}" +
              $"&sort_column={sortColumn}" +
              $"&sort_order={sortOrder}" +
              $"&filters[player_creation_type]=TRACK"
            : $"{BaseUrl}/tracks.xml" +
              $"?page={page}" +
              $"&per_page={perPage}" +
              $"&platform={platform}" +
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
                PlayerId = (int?)x.Attribute("player_id") ?? 0,
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

    public async Task<int> GetPlayerCountAsync(bool? isMnr = null)
    {
        var url = $"{BaseUrl}/api/playercounts/sessioncount";
        if (isMnr.HasValue) url += $"?isMnr={isMnr.Value.ToString().ToLower()}";
        try { return int.Parse(await http.GetStringAsync(url)); }
        catch { return 0; }
    }
    public async Task<PlayerProfile?> GetPlayerInfoAsync(int playerId)
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/players/{playerId}/info.xml");
            var doc = XDocument.Parse(xml);
            var x = doc.Descendants("player").FirstOrDefault();
            if (x == null) return null;

            return new PlayerProfile
            {
                PlayerId = (int?)x.Attribute("player_id") ?? playerId,
                Username = (string?)x.Attribute("username") ?? "",
                Quote = (string?)x.Attribute("quote") ?? "",
                Hearts = (int?)x.Attribute("hearts") ?? 0,
                OnlineRaces = (int?)x.Attribute("online_races") ?? 0,
                OnlineWins = (int?)x.Attribute("online_wins") ?? 0,
                OnlineFinished = (int?)x.Attribute("online_finished") ?? 0,
                OnlineForfeit = (int?)x.Attribute("online_forfeit") ?? 0,
                OnlineDisconnected = (int?)x.Attribute("online_disconnected") ?? 0,
                LongestWinStreak = (int?)x.Attribute("longest_win_streak") ?? 0,
                WinStreak = (int?)x.Attribute("win_streak") ?? 0,
                TotalTracks = (int?)x.Attribute("total_tracks") ?? 0,
                TotalCharacters = (int?)x.Attribute("total_characters") ?? 0,
                TotalKarts = (int?)x.Attribute("total_karts") ?? 0,
                SkillLevel = (string?)x.Attribute("skill_level_name") ?? "",
                Points = (float?)x.Attribute("points") ?? 0,
                CreatorPoints = (float?)x.Attribute("creator_points") ?? 0,
                ExperiencePoints = (float?)x.Attribute("experience_points") ?? 0,
                Presence = (string?)x.Attribute("presence") ?? "",
                LongestDrift = (float?)x.Attribute("longest_drift") ?? 0,
                LongestHangTime = (string?)x.Attribute("longest_hang_time") ?? "",
                CreatedAt = (string?)x.Attribute("created_at") ?? "",
                Rating = (string?)x.Attribute("rating") ?? "",
            };
        }
        catch { return null; }
    }

    public async Task<int> GetPlayerIdAsync(string username)
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/players/to_id.xml?username={Uri.EscapeDataString(username)}");
            var doc = XDocument.Parse(xml);
            return (int?)doc.Descendants("player_id").FirstOrDefault() ?? 0;
        }
        catch { return 0; }
    }

    public async Task<TrackLeaderboard> GetTrackLeaderboardAsync(
    int trackId,
    int page = 1,
    int perPage = 20,
    bool isMnr = false,
    // LBPK: 701=RACE, 702=BATTLE, 704=SCORE_ATTACK
    // For MNR use 701 too
    int gameType = 701,
    int playgroupSize = 1)
    {
        // sort_column=finish_time for time trial, score for others
        var url = $"{BaseUrl}/sub_leaderboards/view.xml" +
                  $"?sub_key_id={trackId}" +
                  $"&sub_group_id={gameType}" +
                  $"&type=OVERALL" +
                  $"&platform=PS3" +
                  $"&page={page}" +
                  $"&per_page={perPage}" +
                  $"&column_page=1" +
                  $"&cols_per_page={perPage}" +
                  $"&sort_column=finish_time" +
                  $"&sort_order=asc" +
                  $"&limit={perPage}" +
                  $"&playgroup_size={playgroupSize}";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("sub_leaderboard").FirstOrDefault();
            var result = new TrackLeaderboard
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Entries = doc.Descendants("player")
                .Select(x => new LeaderboardEntry
                {
                    Rank = (int?)x.Attribute("rank") ?? 0,
                    PlayerId = (int?)x.Attribute("player_id") ?? 0,
                    Username = (string?)x.Attribute("username") ?? "",
                    Score = (float?)x.Attribute("score") ?? 0,
                    FinishTime = (float?)x.Attribute("finish_time") ?? 0,
                    BestLapTime = (float?)x.Attribute("best_lap_time") ?? 0,
                    UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                })
                .ToList();

            return result;
        }
        catch { return new TrackLeaderboard(); }
    }

    public async Task<TrackCreation?> GetTrackByIdAsync(int id)
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/tracks/{id}.xml?is_counted=false");
            var doc = XDocument.Parse(xml);
            var x = doc.Descendants("player_creation").FirstOrDefault();
            if (x == null) return null;

            return new TrackCreation
            {
                Id = (int?)x.Attribute("id") ?? 0,
                PlayerId = (int?)x.Attribute("player_id") ?? 0,
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
            };
        }
        catch { return null; }
    }
    public async Task<CreationsPage> GetTracksByUsernameAsync(string username, bool isMnr = false, string platform = "PS3")
    {
        var url = isMnr
            ? $"{BaseUrl}/player_creations/search.xml?filters[player_creation_type]=TRACK&filters[username]={Uri.EscapeDataString(username)}&per_page=100&platform={platform}&sort_column=created_at&sort_order=desc"
            : $"{BaseUrl}/tracks.xml?filters%5Busername%5D={Uri.EscapeDataString(username)}&per_page=100&platform={platform}&sort_column=created_at&sort_order=desc";
        Console.WriteLine($"Fetching: {url}");
        try
        {
            var xml = await http.GetStringAsync(url);
            return ParseCreationsPage(xml);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTracksByUsernameAsync failed: {ex.Message}");
            return new CreationsPage();
        }
    }

    public async Task<CreationsPage> GetCreationsByTypeAsync(
    int page = 1,
    string type = "CHARACTER",
    string platform = "PS3",
    int perPage = 20,
    string sortColumn = "created_at",
    string sortOrder = "desc",
    string? keyword = null)
    {
        var url = $"{BaseUrl}/player_creations/search.xml" +
                  $"?page={page}" +
                  $"&per_page={perPage}" +
                  $"&platform={platform}" +
                  $"&sort_column={sortColumn}" +
                  $"&sort_order={sortOrder}" +
                  $"&filters[player_creation_type]={type}";

        if (!string.IsNullOrWhiteSpace(keyword))
            url += $"&search={Uri.EscapeDataString(keyword)}";

        try
        {
            var xml = await http.GetStringAsync(url);
            return ParseCreationsPage(xml);
        }
        catch { return new CreationsPage(); }
    }

    // player_creations/search.xml with filters[player_id] works unauthenticated
    // unlike the profile endpoint which uses session context for MNR counts
    // why is it like this jack??? answer me!!! 
    // yk its probably something related to UFG...
    // and I'm not going to put lbpk logic in here because I am too lazy to rewrite code.
    public async Task<int> GetCreationCountAsync(int playerId, string type, string platform = "PS3")
    {
        try
        {
            var url = $"{BaseUrl}/player_creations/search.xml" +
                      $"?page=1&per_page=1&platform={platform}" +
                      $"&filters[player_creation_type]={type}" +
                      $"&filters[player_id]={playerId}";
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);
            return (int?)doc.Descendants("player_creations").FirstOrDefault()?.Attribute("total") ?? 0;
        }
        catch { return 0; }
    }

    public async Task<CreationsPage> GetTeamPicksAsync(bool isMnr = false, string platform = "PS3", int perPage = 40)
    {
        var url = isMnr
            ? $"{BaseUrl}/player_creations/team_picks.xml?per_page={perPage}&platform={platform}&sort_column=created_at&sort_order=desc&filters[player_creation_type]=TRACK"
            : $"{BaseUrl}/tracks/ufg_picks.xml?per_page={perPage}&platform={platform}&sort_column=created_at&sort_order=desc";

        try
        {
            var xml = await http.GetStringAsync(url);
            return ParseCreationsPage(xml);
        }
        catch { return new CreationsPage(); }
    }

    public async Task<TrackCreation?> GetCreationByIdAsync(int id)
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/player_creations/{id}.xml?is_counted=false");
            var doc = XDocument.Parse(xml);
            var x = doc.Descendants("player_creation").FirstOrDefault();
            if (x == null) return null;

            return new TrackCreation
            {
                Id = (int?)x.Attribute("id") ?? 0,
                PlayerId = (int?)x.Attribute("player_id") ?? 0,
                Name = (string?)x.Attribute("name") ?? "Unnamed",
                Username = (string?)x.Attribute("username") ?? "Unknown",
                Description = (string?)x.Attribute("description") ?? "",
                Downloads = (int?)x.Attribute("downloads") ?? 0,
                Hearts = (int?)x.Attribute("hearts") ?? 0,
                RatingUp = (int?)x.Attribute("rating_up") ?? 0,
                RatingDown = (int?)x.Attribute("rating_down") ?? 0,
                Views = (int?)x.Attribute("views") ?? 0,
                RacesStarted = (int?)x.Attribute("races_started") ?? 0,
                Tags = (string?)x.Attribute("tags") ?? "",
                IsTeamPick = (bool?)x.Attribute("is_team_pick") ?? false,
                FirstPublished = (string?)x.Attribute("first_published") ?? "",
                UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                Platform = (string?)x.Attribute("platform") ?? "",
            };
        }
        catch { return null; }
    }

    public async Task<PhotosPage> GetPhotosAsync(int page = 1, int perPage = 40, string? username = null)
    {
        var url = $"{BaseUrl}/photos/search.xml?page={page}&per_page={perPage}";
        if (!string.IsNullOrWhiteSpace(username))
            url += $"&username={Uri.EscapeDataString(username)}";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);
            var wrapper = doc.Descendants("photos").FirstOrDefault();

            return new PhotosPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("current_page") ?? 1,
                Photos = doc.Descendants("photo").Select(x => new PhotoEntry
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    Username = (string?)x.Attribute("username") ?? "",
                    TrackId = (int?)x.Attribute("track_id") ?? 0,
                    AssociatedUsernames = (string?)x.Attribute("associated_usernames") ?? "",
                }).ToList()
            };
        }
        catch { return new PhotosPage(); }
    }

    public async Task<string> GetEulaAsync()
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/policies/view.xml?policy_type=0&platform=PS3&username=ufg");
            var doc = XDocument.Parse(xml);
            return (string?)doc.Descendants("policy").FirstOrDefault()?.Attribute("text") ?? "";
        }
        catch { return ""; }
    }
}