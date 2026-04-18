namespace PLGarageFrontend.Models;

public class TrackCreation
{
    public int Id { get; set; }
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

    public string PreviewImageUrl(string baseUrl) =>
        $"{baseUrl.TrimEnd('/')}/player_creations/{Id}/preview_image.png";
}

public class CreationsPage
{
    public List<TrackCreation> Tracks { get; set; } = [];
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
}