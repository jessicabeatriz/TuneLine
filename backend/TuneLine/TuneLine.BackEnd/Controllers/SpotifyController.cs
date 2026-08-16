using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace TuneLine.BackEnd.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Uri redirectUri = new Uri("https://127.0.0.1:7242/api/spotify/callback");

        public SpotifyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var loginRequest = new LoginRequest(
                redirectUri,
                _configuration["Spotify:ClientId"]!,
                LoginRequest.ResponseType.Code
            );

            loginRequest.Scope = new[] { Scopes.Streaming, Scopes.UserReadEmail, Scopes.UserReadPrivate };

            var urlDeLogin = loginRequest.ToUri().ToString();

            return Ok(new { url = urlDeLogin });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            try
            {
                var request = new AuthorizationCodeTokenRequest(
                    _configuration["Spotify:ClientId"]!,
                    _configuration["Spotify:ClientSecret"]!,
                    code,
                    redirectUri);

                var response = await new OAuthClient().RequestToken(request);

                return Ok(new
                {
                    message = "Sucesso",
                    acess_token = response.AccessToken,
                    refresh_token = response.RefreshToken
                });

            }
            catch (Exception erro)
            {
                return BadRequest(new
                {
                    message = "Não foi possível trocar o código pelo token, o código pode ter expirado.",
                    details = erro.Message
                });
            }
        }

        [HttpGet("perfil")]
        public async Task<IActionResult> GetUserProfile(string token)
        {
            try
            {
                var spotify = new SpotifyClient(token);

                var profile = await spotify.UserProfile.Current();

                return Ok(new
                {
                    name = profile.DisplayName,
                    email = profile.Email,
                    plan = profile.Product
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Erro ao buscar perfil",
                    details = ex.Message                });
            }
        }
        
        [HttpGet("track/{id}")]
        public async Task<IActionResult> GetTrack (string token, string id)
        {
            try
            {
                var spotify = new SpotifyClient(token);
                
                var track = await spotify.Tracks.Get(id);

                return Ok(new
                {
                    name = track.Name,
                    artists = track.Artists.Select(artist => artist.Name).ToList(),
                    album = track.Album.Images[0],
                    link = track.ExternalUrls["spotify"],
                    release_date = track.Album.ReleaseDate
                });

            }
            catch(Exception ex)
            {
                return BadRequest(
                    new
                    {
                        message = "Erro ao buscar música",
                        details = ex.Message
                    });
            }
        }

        [HttpGet("users-track")]
        public async Task<IActionResult> GetUsersSavedTracks (string token)
        {
            try
            {
                var spotify = new SpotifyClient(token);
                var songs = spotify.Library.GetTracks();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }
        
    }
}
