namespace PLGarageFrontend.Models;

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
    public string FirstPublished { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public string Platform { get; set; } = "";
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