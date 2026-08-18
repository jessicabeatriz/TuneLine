using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using TuneLine.BackEnd.Services;

namespace TuneLine.BackEnd.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SpotifyService _spotifyService;
        private readonly Uri redirectUri = new Uri("https://127.0.0.1:7242/api/spotify/callback");

        public SpotifyController(IConfiguration configuration, SpotifyService spotifyService)
        {
            _configuration = configuration;
            _spotifyService = spotifyService;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var loginRequest = new LoginRequest(
                redirectUri,
                _configuration["Spotify:ClientId"]!,
                LoginRequest.ResponseType.Code
            );

            loginRequest.Scope = new[] { Scopes.Streaming, Scopes.UserReadEmail, Scopes.UserReadPrivate, Scopes.UserLibraryRead, Scopes.UserFollowRead, Scopes.PlaylistReadPrivate };

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
                var spotify = new SpotifyClient(response.AccessToken);
                var user = await spotify.UserProfile.Current();

                await _spotifyService.SaveOrUpdateUserAsync(
                    user.Id,
                    response.AccessToken,
                    response.RefreshToken,
                    response.ExpiresIn
                    );

                return Ok(new
                {
                    message = "Usuário logado e salvo com sucesso",
                    name = user.DisplayName,
                    image = user.Images.Select(i => i.Url).FirstOrDefault()
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
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            try
            {
                var profile = await _spotifyService.GetUserProfileAsync(userId);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Erro ao buscar perfil",
                    details = ex.Message
                });
            }
        }
        
        [HttpGet("track/{id}")]
        public async Task<IActionResult> GetTrack (string userId, string id)
        {
            try
            {
                var track = await _spotifyService.GetTrackAsync(userId, id);

                return Ok(track);

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

        [HttpGet("games-tracks")]
        public async Task<IActionResult> GetTracksForGame(string userId)
        {
            try
            {
                var gameData = await _spotifyService.GetTracksForGameAsync(userId);
                return Ok(gameData);
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = "Erro ao gerar músicas para o jogo",
                    details = ex.Message
                });
            }
        }
        
    }
}
