namespace PLGarageFrontend.Models;

// this is supposed to be for tracks only, but we just repurposed it for the general models.
public class TrackCreation
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Description { get; set; } = "";
    public int Downloads { get; set; }
    public int Hearts { get; set; }
    public int RatingUp { get; set; }
    public int RatingDown { get; set; }
    public int Views { get; set; }
    public int RacesStarted { get; set; }
    public int NumLaps { get; set; }
    public int NumRacers { get; set; }
    public string RaceType { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Tags { get; set; } = "";
    public bool IsTeamPick { get; set; }
    public bool IsRemixable { get; set; }
    public bool IsMNR { get; set; }
    public string FirstPublished { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Game { get; set; } = "";

}

public class CreationsPage
{
    public List<TrackCreation> Tracks { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class PlayerProfile
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = "";
    public string Quote { get; set; } = "";
    public int Hearts { get; set; }
    public int OnlineRaces { get; set; }
    public int OnlineWins { get; set; }
    public int OnlineFinished { get; set; }
    public int OnlineForfeit { get; set; }
    public int OnlineDisconnected { get; set; }
    public int LongestWinStreak { get; set; }
    public int WinStreak { get; set; }
    public int TotalTracks { get; set; }
    public int TotalCharacters { get; set; }
    public int TotalKarts { get; set; }
    public string SkillLevel { get; set; } = "";
    public float Points { get; set; }
    public float CreatorPoints { get; set; }
    public float ExperiencePoints { get; set; }
    public string Presence { get; set; } = "";
    public float LongestDrift { get; set; }
    public string LongestHangTime { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string Rating { get; set; } = "";
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public int PlayerId { get; set; }
    public string Username { get; set; } = "";
    public float Score { get; set; }
    public float FinishTime { get; set; }
    public float BestLapTime { get; set; }
    public string UpdatedAt { get; set; } = "";
}

public class TrackLeaderboard
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public List<LeaderboardEntry> Entries { get; set; } = [];
}

public class PhotoEntry
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int TrackId { get; set; }
    public string AssociatedUsernames { get; set; } = "";
}

public class PhotosPage
{
    public List<PhotoEntry> Photos { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class Lobby
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TrackId { get; set; }
    public int HostPlayerId { get; set; }
    public int CurPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int MinPlayers { get; set; }
    public string GameType { get; set; } = "";
    public int GameStateId { get; set; }
    public string SpeedClass { get; set; } = "";
    public int NumberLaps { get; set; }
    public bool IsRanked { get; set; }

    public bool IsPending => GameStateId == 0;
    public string Game => GameType switch
    {
        "RACE" or "BATTLE" or "SCORE_ATTACK" or "BUBBLE_CHASE" => "LBPK",
        _ => "MNR"
    };
}

public class LobbyPage
{
    public List<Lobby> Lobbies { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class Comment
{
    public int Id { get; set; }
    public int PlayerCreationId { get; set; }
    public int PlayerId { get; set; }
    public int AuthorId { get; set; }
    public string Username { get; set; } = "";
    public string AuthorUsername { get; set; } = "";
    public string Body { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public int RatingUp { get; set; }
    public int RatingDown { get; set; }
    public string Platform { get; set; } = "";
}

public class CommentsPage
{
    public List<Comment> Comments { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public int PlayerCreationId { get; set; }
    public int PlayerId { get; set; }
    public string Username { get; set; } = "";
    public string Content { get; set; } = "";
    public string PlayerCreationName { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }
    public string Tags { get; set; } = "";
}

public class ReviewsPage
{
    public List<Review> Reviews { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}

public class ActivityEntry
{
    public string Type { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Details { get; set; } = "";
    public string CreatorUsername { get; set; } = "";
    public int CreatorId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerUsername { get; set; } = "";
    public string PlayerCreationName { get; set; } = "";
    public int PlayerCreationId { get; set; }
    public string Timestamp { get; set; } = "";
}

public class ActivityPage
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public List<ActivityEntry> Activities { get; set; } = new();
}