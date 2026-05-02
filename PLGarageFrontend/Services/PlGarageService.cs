using Microsoft.AspNetCore.Mvc.ViewEngines;
using PLGarageFrontend.Models;
using System.Xml.Linq;

namespace PLGarageFrontend.Services;

public class PLGarageService(HttpClient http)
{

    public string BaseUrl => File.ReadAllLines("config.txt")
    .Select(line => line.Split('='))
    .FirstOrDefault(parts => parts[0] == "BaseUrl")?[1] ?? "";

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
        // This is really weird decision by UFG
        // Thanks, UFG!
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
    public async Task<CreationsPage> GetTracksByUsernameAsync(string username, bool isMnr = false, string platform = "PS3", string type = "TRACK")
    {
        var url = isMnr
            ? $"{BaseUrl}/player_creations/search.xml?filters[player_creation_type]={Uri.EscapeDataString(type)}&filters[username]={Uri.EscapeDataString(username)}&per_page=100&platform={platform}&sort_column=created_at&sort_order=desc"
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
            ? $"{BaseUrl}/player_creations/team_picks.xml?page=1&per_page={perPage}&platform={platform}&sort_column=created_at&sort_order=desc&filters[player_creation_type]=TRACK"
            : $"{BaseUrl}/tracks/ufg_picks.xml?filters[player_creation_type]=TRACK&page=1&per_page={perPage}&platform={platform}";

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

    public async Task<LobbyPage> GetLobbiesAsync(int page = 1, int perPage = 50)
    {
        var url = $"{BaseUrl}/multiplayer_games.xml?page={page}&per_page={perPage}";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("games").FirstOrDefault();
            var result = new LobbyPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Lobbies = doc.Descendants("game")
                .Select(x => new Lobby
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    Name = (string?)x.Attribute("name") ?? "",
                    TrackId = (int?)x.Attribute("track") ?? 0,
                    HostPlayerId = (int?)x.Attribute("host_player_id") ?? 0,
                    CurPlayers = (int?)x.Attribute("cur_players") ?? 0,
                    MaxPlayers = (int?)x.Attribute("max_players") ?? 0,
                    MinPlayers = (int?)x.Attribute("min_players") ?? 0,
                    GameType = (string?)x.Attribute("game_type") ?? "",
                    GameStateId = (int?)x.Attribute("game_state_id") ?? 0,
                    SpeedClass = (string?)x.Attribute("speed_class") ?? "",
                    NumberLaps = (int?)x.Attribute("number_laps") ?? 0,
                    IsRanked = (bool?)x.Attribute("is_ranked") ?? false,
                })
                .Where(g => g.GameType != "CHARACTER_CREATORS") // filter out ModSpot
                .ToList();

            return result;
        }
        catch { return new LobbyPage(); }
    }

    public async Task<CreationsPage> GetTracksByPlayerIdAsync(int playerId, bool isMnr = false, string platform = "PS3", string username = "", string type = "TRACK")
    {
        string url;
        if (isMnr)
            url = $"{BaseUrl}/player_creations/friends_view.xml" +
                  $"?filters[player_creation_type]={Uri.EscapeDataString(type)}" +
                  $"&filters[username]={Uri.EscapeDataString(username)}" +
                  $"&page=1&per_page=100&platform={platform}" +
                  $"&sort_column=created_at&sort_order=desc";
        else
            url = $"{BaseUrl}/tracks.xml" +
                  $"?filters[username]={Uri.EscapeDataString(username)}" +
                  $"&page=1&per_page=100&platform={platform}" +
                  $"&sort_column=created_at&sort_order=desc";

        try
        {
            var xml = await http.GetStringAsync(url);
            return ParseCreationsPage(xml);
        }
        catch { return new CreationsPage(); }
    }

    public async Task<CommentsPage> GetTrackCommentsAsync(int trackId, int page = 1, int perPage = 20)
    {
        var url = $"{BaseUrl}/player_creation_comments.xml" +
                  $"?filters[player_creation_id]={trackId}" +
                  $"&page={page}&per_page={perPage}" +
                  $"&sort_column=created_at&sort_order=desc";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("player_creation_comments").FirstOrDefault();
            var result = new CommentsPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Comments = doc.Descendants("player_creation_comment")
                .Select(x => new Comment
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    PlayerCreationId = (int?)x.Attribute("player_creation_id") ?? 0,
                    PlayerId = (int?)x.Attribute("player_id") ?? 0,
                    Username = (string?)x.Attribute("username") ?? "",
                    Body = (string?)x.Attribute("body") ?? "",
                    CreatedAt = (string?)x.Attribute("created_at") ?? "",
                    UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                    RatingUp = (int?)x.Attribute("rating_up") ?? 0,
                    RatingDown = (int?)x.Attribute("rating_down") ?? 0,
                    Platform = (string?)x.Attribute("platform") ?? "",
                })
                .ToList();

            return result;
        }
        catch { return new CommentsPage(); }
    }

    public async Task<CommentsPage> GetProfileCommentsAsync(int playerId, int page = 1, int perPage = 20)
    {
        var url = $"{BaseUrl}/player_comments.xml" +
                  $"?filters[player_id]={playerId}" +
                  $"&page={page}&per_page={perPage}" +
                  $"&sort_column=created_at&sort_order=desc";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("player_comments").FirstOrDefault();
            var result = new CommentsPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Comments = doc.Descendants("player_comment")
                .Select(x => new Comment
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    AuthorId = (int?)x.Attribute("author_id") ?? 0,
                    PlayerId = (int?)x.Attribute("player_id") ?? 0,
                    Username = (string?)x.Attribute("author_username") ?? "",
                    Body = (string?)x.Attribute("body") ?? "",
                    CreatedAt = (string?)x.Attribute("created_at") ?? "",
                    UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                    RatingUp = (int?)x.Attribute("rating_up") ?? 0,
                    RatingDown = (int?)x.Attribute("rating_down") ?? 0,
                    Platform = (string?)x.Attribute("platform") ?? "",
                })
                .ToList();

            return result;
        }
        catch { return new CommentsPage(); }
    }


    // dev1:
    // doesn't look like this returns anything, but the endpoint exists, but returns nothing, so i will omit reviews for now
    // dev2: it does 

    // dev2: 
    // but actualy for some reason it worked before and now it returns this:
    // This page contains the following errors:
    // error on line 1 at column 499: xmlParseCharRef: invalid xmlChar value 0
    // Below is a rendering of the page up to the first error.
    // 0Successful completion

    // dev1:
    // strange lmao
    
    public async Task<ReviewsPage> GetTrackReviewsAsync(int trackId, int page = 1, int perPage = 20)
    {
        var url = $"{BaseUrl}/player_creation_reviews.xml" +
                  $"?player_creation_id={trackId}" +
                  $"&page={page}&per_page={perPage}";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("player_creation_reviews").FirstOrDefault();
            var result = new ReviewsPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Reviews = doc.Descendants("review")
                .Select(x => new Review
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    PlayerCreationId = (int?)x.Attribute("player_creation_id") ?? 0,
                    PlayerId = (int?)x.Attribute("player_id") ?? 0,
                    Username = (string?)x.Attribute("username") ?? "",
                    Content = (string?)x.Attribute("content") ?? "",
                    PlayerCreationName = (string?)x.Attribute("player_creation_name") ?? "",
                    CreatedAt = (string?)x.Attribute("created_at") ?? "",
                    UpdatedAt = (string?)x.Attribute("updated_at") ?? "",
                    ThumbsUp = (int?)x.Attribute("thumbs_up") ?? 0,
                    ThumbsDown = (int?)x.Attribute("thumbs_down") ?? 0,
                    Tags = (string?)x.Attribute("tags") ?? "",
                })
                .ToList();

            return result;
        }
        catch { return new ReviewsPage(); }
    }

    public async Task<ActivityPage> GetActivityLogAsync(int playerId, int page = 1, int perPage = 20)
    {
        var url = $"{BaseUrl}/activity_log.xml?player_id={playerId}&page={page}&per_page={perPage}";

        try
        {
            var xml = await http.GetStringAsync(url);
            var doc = XDocument.Parse(xml);

            var wrapper = doc.Descendants("activities").FirstOrDefault();
            var result = new ActivityPage
            {
                Total = (int?)wrapper?.Attribute("total") ?? 0,
                TotalPages = (int?)wrapper?.Attribute("total_pages") ?? 0,
                Page = (int?)wrapper?.Attribute("page") ?? 1,
            };

            result.Activities = doc.Descendants("activity")
                .SelectMany(a => a.Descendants("event").Select(e => new ActivityEntry
                {
                    Type = (string?)a.Attribute("type") ?? "",
                    Topic = (string?)e.Attribute("type") ?? "",
                    Details = (string?)e.Attribute("details") ?? "",
                    CreatorUsername = (string?)e.Attribute("creator_username") ?? "",
                    CreatorId = (int?)e.Attribute("creator_id") ?? 0,
                    PlayerId = (int?)e.Attribute("player_id") ?? 0,
                    PlayerUsername = (string?)a.Attribute("player_username") ?? "",
                    PlayerCreationName = (string?)a.Attribute("player_creation_name") ?? "",
                    PlayerCreationId = (int?)a.Attribute("player_creation_id") ?? 0,
                    Timestamp = (string?)e.Attribute("timestamp") ?? "",
                }))
                .ToList();

            return result;
        }
        catch { return new ActivityPage(); }
    }

    public async Task<List<AnnouncementEntry>> GetAnnouncementsAsync(string platform = "WEB")
    {
        try
        {
            var xml = await http.GetStringAsync($"{BaseUrl}/announcements.xml?platform={platform}");
            var doc = XDocument.Parse(xml);
            return doc.Descendants("announcement")
                .Select(x => new AnnouncementEntry
                {
                    Id = (int?)x.Attribute("id") ?? 0,
                    Subject = (string?)x.Attribute("subject") ?? "",
                    Text = x.Value,
                    LanguageCode = (string?)x.Attribute("language_code") ?? "",
                    CreatedAt = (string?)x.Attribute("created_at") ?? "",
                })
                .ToList();
        }
        catch { return new List<AnnouncementEntry>(); }
    }
}