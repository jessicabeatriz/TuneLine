using Microsoft.AspNetCore.Http.HttpResults;
using SpotifyAPI.Web;
using System.Diagnostics;
using TuneLine.BackEnd.Models;
using TuneLine.BackEnd.Repositories;

namespace TuneLine.BackEnd.Services
{
    public class SpotifyService
    {
        private readonly IUserRepository _userRepository; 
        private readonly IConfiguration _configuration;
        public SpotifyService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task SaveOrUpdateUserAsync(string spotifyId, string accessToken, string refreshToken, int expiresInSeconds)
        {
            var expirationDate = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            var user = await _userRepository.GetByIdAsync(spotifyId);

            if (user == null)
            {
                user = new User
                {
                    Id = spotifyId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpirationDate = expirationDate
                };
                await _userRepository.AddAsync(user);
            }
            else
            {
                user.AccessToken = accessToken;

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    user.RefreshToken = refreshToken;
                }

                user.ExpirationDate = expirationDate;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<string> GetValidAccessTokenAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if(user == null)
            {
                throw new Exception("Usuário não encontrado");
            }

            if(user.ExpirationDate > DateTime.UtcNow.AddMinutes(1))
            {
                return user.AccessToken;
            }
            else
            {
                var request = new AuthorizationCodeRefreshRequest(
                    _configuration["Spotify:ClientId"]!,
                    _configuration["Spotify:ClientSecret"]!,
                    user.RefreshToken
                    );

                var response = await new OAuthClient().RequestToken(request);

                if (string.IsNullOrEmpty(response.AccessToken))
                    throw new Exception("O Spotify não retornou um Access Token válido durante a renovação");
                else
                    user.AccessToken = response.AccessToken;

                if (!string.IsNullOrEmpty(response.RefreshToken))
                    user.RefreshToken = response.RefreshToken;
                
                user.ExpirationDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

                await _userRepository.UpdateAsync(user);
                                
                return user.AccessToken;

            }
        }

        public async Task<object> GetUserProfileAsync(string userId)
        {
            var token = await GetValidAccessTokenAsync(userId);
            var spotify = new SpotifyClient(token);
            var profile = await spotify.UserProfile.Current();

            return new
            {
                name = profile.DisplayName,
                email = profile.Email,
                plan = profile.Product
            };
        }

        public async Task<object> GetTrackAsync(string userId, string trackId)
        {
            var token = await GetValidAccessTokenAsync(userId);
            var spotify = new SpotifyClient(token);
            var track = await spotify.Tracks.Get(trackId);

            return new
            {
                name = track.Name,
                artists = track.Artists.Select(artist => artist.Name).ToList(),
                album = track.Album.Images.FirstOrDefault()?.Url,
                link = track.ExternalUrls["spotify"],
                release_date = track.Album.ReleaseDate
            };
        }

        public async Task<object> GetTracksForGameAsync(string userId)
        {
            var token = await GetValidAccessTokenAsync(userId);
            var spotify = new SpotifyClient(token);

            var request = new LibraryTracksRequest {  Limit = 50 };
            var firstPage = await spotify.Library.GetTracks(request);

            var paginateSavedTracks = await spotify.PaginateAll(firstPage);
            var savedTracks = paginateSavedTracks.Select(track => new
            {
                name = track.Track.Name,
                artists = track.Track.Artists.Select(artist => artist.Name).ToList(),
                album = track.Track.Album.Name,
                image = track.Track.Album.Images.FirstOrDefault()?.Url,
                release_year = track.Track.Album.ReleaseDate.Substring(0, 4),
                link = track.Track.ExternalUrls.ContainsKey("spotify") ? track.Track.ExternalUrls["spotify"] : null
            }).ToList();

            var random = new Random();
            var gameTracks = savedTracks
                .OrderBy(x => random.Next())
                .GroupBy(x => x.release_year)
                .Select(group => group.First())
                .Take(20)
                .ToList();

            return new
            {
                total_library_size = paginateSavedTracks.Count,
                total_game_tracks = gameTracks.Count,
                tracks = gameTracks
            };
        }

    }
}
