using GoogleMusicWebClientAPI;
using GoogleMusicWebClientAPI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
namespace GoogleMusicWebClientAPI
{
    public interface IGoogleMusicWebClient
    {
        Task<bool> Login(string email, string password);
        bool HasSession();
        Task<bool> HitForSessionCookies();
        bool NeedsAuth();

        Task<bool> AddPlaylist(string playlistName);
        Task<bool> GetUserPlaylists();
        
        Task<IEnumerable<GoogleMusicSong>> GetAllSongs();
        Task<IEnumerable<GoogleMusicSong>> GetRecommendedForYou();
        Task<IEnumerable<GoogleMusicSong>> FetchTracks(IEnumerable<GoogleMusicSong> tracks);
        Task<bool> ModifyTrack(GoogleMusicSongDiff track);
        Task<string> GetStreamURL(GoogleMusicSong song);
        Task<string> GetShareableURL(GoogleMusicSong song);
        void GetSharedWithMe(ObservableCollection<GoogleMusicSong> sharedSongs);
        Task<int> GetTrackCount();
        Task<GoogleMusicSearchResults> Search(string query);

        void IncrementPlaycount(GoogleMusicSong song);
        void DislikeSong(GoogleMusicSong song);
        void LikeSong(GoogleMusicSong song);
    }
}
