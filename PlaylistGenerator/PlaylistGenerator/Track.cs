namespace PlaylistGenerator;

public class Track
{
    public string Name { get; init; }
    public Artist Artist { get; init; }

    public Track(string name, Artist artist)
    {
       Name = name;
       Artist = artist;
    }

    
    
}
