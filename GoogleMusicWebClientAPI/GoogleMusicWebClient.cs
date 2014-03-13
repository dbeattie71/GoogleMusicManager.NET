using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;

using Byteopia.Helpers;
using GoogleMusicWebClientAPI.StreamingLoadAllTracks;
using GoogleMusicWebClientAPI.HttpHandlers;
using GoogleMusicWebClientAPI.Models;
using GoogleMusicWebClientAPI.RecommendedForYou;
using Newtonsoft.Json;


namespace GoogleMusicWebClientAPI
{
    /// <summary>
    /// Super small wrapper for Google's API
    /// </summary>
    /// 
    [DataContract]
    public class GoogleMusicWebClient : IGoogleMusicWebClient
    {
        private ISessionStorage Settings { get; set; }

        private GoogleHTTP Client { get; set; }

        private IGoogleCookieManager cookieManager { get; set; }

        private AuthorizationTokenHttpHandler authorizationTokenHandler { get; set; }

        public GoogleMusicWebClient(ISessionStorage settings)
        {
            this.Settings = settings;
            var cookieHandler = new CookieHttpHandler();
            this.cookieManager = cookieHandler;
            this.authorizationTokenHandler = new AuthorizationTokenHttpHandler();
            this.Client = new GoogleHTTP(authorizationTokenHandler, cookieHandler);
        }

        public ObservableCollection<GoogleMusicPlaylist> Playlists = new ObservableCollection<GoogleMusicPlaylist>();

        public bool HasSession()
        {
            return this.DeseralizeSession();
        }

        /// <summary>
        /// Login via email and pw
        /// </summary>
        /// <param name="email">Google music user email</param>
        /// <param name="password">Google music user pass</param>
        /// <returns></returns>
        public async Task<Boolean> Login(String email, String password)
        {
            bool hasSession = this.DeseralizeSession();
            if (hasSession)
                return true;

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("service", "sj"), // skyjam
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Passwd", password)
            });

            // First hit for auth token
            String loginData = await this.Client.POST(new Uri("https://accounts.google.com/ClientLogin"), content);

            // Bad creds, prolly
            if (this.Client.LastStatusCode == HttpStatusCode.Forbidden)
                return false;

            this.authorizationTokenHandler.SetAuthToken(loginData);

            // Hit the servers so our cookie container can store the cookies
            await HitForSessionCookies();

            await this.SeralizeSession();

            return true;
        }

        public async Task<int> GetTrackCount()
        {
            GoogleMusicStatus status = await this.Client.POST<GoogleMusicStatus>(new Uri("https://play.google.com/music/services/getstatus"));
            return status.TotalTracks;
        }

        /// <summary>
        /// Gets all songs in music library
        /// </summary>
        /// <param name="continuationToken">Tells Google's servers where to pick up</param>
        /// <returns></returns>
        public async Task<IEnumerable<GoogleMusicSong>> GetAllSongs()
        {
            var url = @"https://play.google.com/music/services/streamingloadalltracks?u=0&xt={0}&json={""tier"":1,""requestCause"":1,""requestType"":1,""sessionId"":""{1}""}&format=jsarray";
            // var sessionId = "23u9sqlx13vn";
            var sessionId = "f4n3098h48h4"; //TODO some session id (client side generated?)

            url = url.Replace("{0}", this.cookieManager.GetXtCookie());
            url = url.Replace("{1}", sessionId);

            var playlist = await this.Client.GET(new Uri(url));

            File.WriteAllText("StreamingLoadAllTracks.html", playlist);

            var processor = new StreamingLoadAllTracksResponseProcessor();
            var results = processor.ProcessStreamingLoadAllTracks(playlist);

            return results;
        }

        /// <summary>
        /// Gets complete list of all playlists
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetUserPlaylists()
        {
            GoogleMusicPlaylists playlists = await this.Client.POST<GoogleMusicPlaylists>(new Uri("https://play.google.com/music/services/loadplaylist"));

            if (playlists.UserPlaylists != null)
            {
                foreach (GoogleMusicPlaylist playlist in playlists.UserPlaylists)
                    Playlists.Add(playlist);
            }

            if (playlists.InstantMixes != null)
            {
                foreach (GoogleMusicPlaylist playlist in playlists.InstantMixes)
                    Playlists.Add(playlist);
            }

            return true;
        }

        /// <summary>
        /// Populates the list of songs based on Google's recommendations
        /// </summary>
        public async Task<IEnumerable<GoogleMusicSong>> GetRecommendedForYou()
        {
            var url = "https://play.google.com/music/services/recommendedforyou?u=0&xt={0}&format=jsarray";
            url = url.Replace("{0}", this.cookieManager.GetXtCookie());
            var content = new StringContent(@"[[""f4n3098h48h4"",1],[]]");
            var recsString = await this.Client.POST(new Uri(url), content);

            var responseProcessor = new RecommendedForYouResponseProcessor();
            var recs = responseProcessor.RecommendedForYouResponse(recsString);

            return recs;
        }

        public async Task<IEnumerable<GoogleMusicSong>> FetchTracks(IEnumerable<GoogleMusicSong> tracks)
        {
            var url = "https://play.google.com/music/services/fetchtracks?u=0&xt={0}&format=jsarray";
            url = url.Replace("{0}", this.cookieManager.GetXtCookie());

            var trackIds = string.Join(@""",""", tracks.Select(p => p.MatchedID));
            var content = @"[[""f4n3098h48h4"",1],[[""" + trackIds + @"""]]]";
            var stringcontent = new StringContent(content);
            var recsString = await this.Client.POST(new Uri(url), stringcontent);

            var responseProcessor = new RecommendedForYouResponseProcessor();
            var recs = responseProcessor.RecommendedForYouResponse(recsString);

            return recs;
        }

        public async Task<bool> ModifyTrack(GoogleMusicSongDiff track)
        {
            var url = "https://play.google.com/music/services/modifytracks?u=0&xt={0}&format=jsarray";
            url = url.Replace("{0}", this.cookieManager.GetXtCookie());

            var diffFields = track.GetDiffArray();
            var diffString = JsonConvert.SerializeObject(diffFields);

            var content = @"[[""f4n3098h48h4"",1],[[" + diffString + @"]]]";
            var stringcontent = new StringContent(content);
            var recsString = await this.Client.POST(new Uri(url), stringcontent);

            return true;
        }

        /// <summary>
        /// Gets songs shared with user via G+
        /// </summary>
        public async void GetSharedWithMe(ObservableCollection<GoogleMusicSong> sharedSongs)
        {
            SharedSongList ssl = await this.Client.POST<SharedSongList>(new Uri("https://play.google.com/music/services/sharedwithme"));

            foreach (GoogleMusicSong song in ssl.Shares)
                sharedSongs.Add((song));
        }

        /// <summary>
        /// Adds playlist
        /// </summary>
        /// <param name="playlistName">The playlist to add</param>
        public async Task<bool> AddPlaylist(String playlistName)
        {
            String jsonString = "{\"title\":\"" + playlistName + "\"}";

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("json", jsonString),
            });

            AddPlaylistResp pl = await this.Client.POST<AddPlaylistResp>(new Uri("https://play.google.com/music/services/addplaylist"), content);

            return pl.Success;
        }

        /// <summary>
        /// Search the music library
        /// </summary>
        /// <param name="query">The query term</param>
        /// <returns></returns>
        public async Task<GoogleMusicSearchResults> Search(String query)
        {
            var url = "https://play.google.com/music/services/search?u=0&xt={0}&format=jsarray";
            url = url.Replace("{0}", this.cookieManager.GetXtCookie());
            var contentString = @"[[""f4n3098h48h4"",1],[""" + query + @""",10]]";

            var content = new StringContent(contentString);

            var searchResults = await this.Client.POST(new Uri(url), content);

            var responseProcssor = new RecommendedForYouResponseProcessor();
            var tracks = responseProcssor.RecommendedForYouResponse(searchResults);


            return new GoogleMusicSearchResults()
            {
                Songs = tracks.ToList(),
            };
        }
        ///// <summary>
        ///// Launches the store results page with a given query
        ///// </summary>
        ///// <param name="query">The query term</param>
        //public async void QueryStore(String query)
        //{
        //    await Windows.System.Launcher.LaunchUriAsync(new Uri(String.Format("https://play.google.com/store/search?c=music&feature=music_play_menu&q={0}", query)));
        //}

        /// <summary>
        /// Thumbs up a song
        /// </summary>
        /// <param name="song">Song to like</param>
        public async void LikeSong(GoogleMusicSong song)
        {
            var diff = new GoogleMusicSongDiff()
            {
                ID = song.ID,
                Rating = 5,
            };
            await this.ModifyTrack(diff);
        }

        /// <summary>
        /// Thumbs down a song
        /// </summary>
        /// <param name="song">Song to hate</param>
        public async void DislikeSong(GoogleMusicSong song)
        {
            var diff = new GoogleMusicSongDiff()
            {
                ID = song.ID,
                Rating = 0,
            };
            await this.ModifyTrack(diff);
        }

        /// <summary>
        /// Increment a song's playcount by 1
        /// </summary>
        /// <param name="song">Song to inc playcount</param>
        public async void IncrementPlaycount(GoogleMusicSong song)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get song share url
        /// </summary>
        /// <param name="song">song to share</param>
        /// <returns></returns>
        public async Task<String> GetShareableURL(GoogleMusicSong song)
        {
            if (song.StoreID == null)
                return null;

            var url = "https://play.google.com/music/m/{0}";
            url = url.Replace("{0}", song.StoreID);
            return url;
        }

        /// <summary>
        /// Gets the stream URL from a given song
        /// </summary>
        /// <param name="song">Song to stream</param>
        /// <returns></returns>
        public async Task<String> GetStreamURL(GoogleMusicSong song)
        {
            Uri reqUrl = null;
            // Looks like it's a preview song
            if (song.Preview != null)
            {
                reqUrl = new Uri(String.Format("https://play.google.com/music/playpreview?u=0&mode=streaming&preview={0}&tid={1}&pt=e",
                    song.Preview.PreviewToken, song.Preview.TrackID));
            }
            // Shared song
            else if (song.Share != null)
            {
                reqUrl = new Uri(String.Format("https://play.google.com/music/playpreview?u=0&mode=streaming&preview={0}&tid={1}&postid={2}pt=e",
                    song.Preview.PreviewToken, song.Preview.TrackID, song.Share.PostID));
            }
            // Normal song
            else
            {
                reqUrl = new Uri(String.Format("https://play.google.com/music/play?u=0&songid={0}", song.ID));
            }

            GoogleMusicSongUrl songUrl = null;
            try
            {
                songUrl = await this.Client.GET<GoogleMusicSongUrl>(reqUrl);
            }
            catch
            {

            }

            return (songUrl != null) ? songUrl.url : String.Empty;
        }

        public async Task<bool> HitForSessionCookies()
        {
            String hitForCookies = await this.Client.POST(new Uri("https://play.google.com/music/listen?hl=en&u=0"));
            return true;
        }

        public bool NeedsAuth()
        {
            return this.authorizationTokenHandler.Equals(String.Empty);
        }

        void client_CookiesChanged(object sender, EventArgs e)
        {
            this.SeralizeSession();
        }

        private async Task<bool> SeralizeSession()
        {
            String googleClient = String.Empty;

            try
            {
                googleClient = JSON.SerializeObject(new Session()
                {
                    AuthToken = this.authorizationTokenHandler.AuthorizationToken,
                    Cookies = this.cookieManager.GetCookiesList()
                });

                Settings.SetSerializedValue("session", googleClient, true);
            }
            catch
            {
                throw;
                return false;
            }

            return true;
        }

        private bool DeseralizeSession()
        {
            Session tmp = null;
            try
            {
                tmp = JSON.Deserialize<Session>(Settings.GetSerializedStringValue("session", true));

                this.authorizationTokenHandler.AuthorizationToken = tmp.AuthToken;
                this.cookieManager.SetCookiesFromList(tmp.Cookies);

            }
            catch
            {
                //throw;
                return false;
            }

            return true;
        }


    }
}