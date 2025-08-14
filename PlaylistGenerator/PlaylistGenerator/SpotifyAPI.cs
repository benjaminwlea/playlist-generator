using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class PlayList
    
{
    static readonly SpotifyAuthHelper authHelper = new(
            "dadcc3e1920f4fb78f62e6704e233a0f",
            "95932a3835fa4aa3824e7407c3e7badd",
            "AQAfWV4EtagD6U7kVzVPtlRn1mGFMvSRmEDdRxmrFgVmiEXI3eG-oC-KUQUJB1Vzphvsslp78v-KCQ8skdjO2PigmtQjQ5m9PsXISdmgB7wr0O9HipCySPnoiUDZdL1JMXA" // obtained during initial OAuth login
        );

    static async Task Main(string[] args)
    {
        var playlistId = "4ftVVVOSYoxupshTo9x8k3";
        var songID = "4uLU6hMCjMI75M1A2tKUQC"; // Example track ID

        // Example: Add 3 songs
        string[] trackUris =
        [
            "spotify:track:4uLU6hMCjMI75M1A2tKUQC",
            "spotify:track:1301WleyT98MSxVHPZCA6M",
            "spotify:track:7lPN2DXiMsVn7XUKtOW1CS"
        ];
        try
        {
            var tmp = await GetTrack("Laid to rest","Lamb of god");//playlistId,trackUris);
            await GetTrack(tmp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    public static async Task AddTracksToPlaylistAsync(string playlistId, string[] trackUris)
    {
 
        var accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var json = JsonSerializer.Serialize(new { uris = trackUris });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            $"https://api.spotify.com/v1/playlists/{playlistId}/tracks",
            content
        );

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Tracks added successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to add tracks: {response.StatusCode}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }

    static async Task MakePlaylist(string playListName)
    {
        var accessToken = await authHelper.GetAccessTokenAsync();
        var userId = "11181725669"; // e.g., from GET /v1/me

        var playlistDescription = "Created via raw HTTP in C#";
        var isPublic = false;

        using var client = new HttpClient();

        // Add the Authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Build the JSON payload
        var json = JsonSerializer.Serialize(new
        {
            name = playListName,
            description = playlistDescription,
            @public = isPublic
        });

        // Wrap JSON string in StringContent with correct media type
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Call the API
        var url = $"https://api.spotify.com/v1/users/{userId}/playlists";
        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Playlist created successfully!");
            Console.WriteLine(responseBody);
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to create playlist. Status: {response.StatusCode}");
            Console.WriteLine(errorBody);
        }
    }
    static async Task<string> GetPlaylist(string id)
    {
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/playlists/{id}");
        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
        return json;
    }

    // Returns Spotify track information for the given track ID
    static async Task GetTrack(string id)
    {
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/tracks/{id}");
        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
    }

    // Returns Spotify artist artist information for the given artist ID
    static async Task GetArtist(string artist)
    {
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"https://api.spotify.com/v1/artists/{artist}");
        string json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
    }


    //Returns Spotify track ID for the given track name and artist
    static async Task<string> GetTrack(string name,string artist)
    {
        string query = "track:"+name+" artist:"+artist;
        string accessToken = await authHelper.GetAccessTokenAsync();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        string url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=1";

        var response = await client.GetAsync(url);
        string json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var item = doc.RootElement
            .GetProperty("tracks")
            .GetProperty("items")[0]
            .GetProperty("id").ToString();

        return item;
    }

}