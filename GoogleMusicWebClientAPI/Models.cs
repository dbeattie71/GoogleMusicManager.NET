using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using GoogleMusicWebClientAPI.Models;


namespace GoogleMusicWebClientAPI
{


    [DataContract]
    public class AddPlaylistResp
    {
        [DataMember(Name = "id")]
        public String ID { get; set; }

        [DataMember(Name = "title")]
        public String Title { get; set; }

        [DataMember(Name = "success")]
        public bool Success { get; set; }
    }

    [DataContract]
    public class DeletePlaylistResp
    {
        [DataMember(Name = "deleteId")]
        public String ID { get; set; }
    }

    [DataContract]
    public class RequestTrackList
    {
        public RequestTrackList(String token)
        {
            ContToken = token;
        }

        [DataMember(Name = "continuationToken")]
        public String ContToken { get; set; }
    }

    [DataContract]
    public class GoogleMusicPlaylists
    {
        [DataMember(Name = "playlists")]
        public List<GoogleMusicPlaylist> UserPlaylists { get; set; }

        [DataMember(Name = "magicPlaylists")]
        public List<GoogleMusicPlaylist> InstantMixes { get; set; }
    }

    [DataContract]
    public class GoogleMusicPlaylist : INotifyPropertyChanged
    {
        string title;
        [DataMember(Name = "title")]
        public string Title
        {
            get { return title; }
            set { title = value; NotifyPropertyChanged("Title"); }
        }

        [DataMember(Name = "playlistId")]
        public string PlaylistID { get; set; }

        [DataMember(Name = "requestTime")]
        public double RequestTime { get; set; }

        [DataMember(Name = "continuationToken")]
        public string ContToken { get; set; }

        [DataMember(Name = "differentialUpdate")]
        public bool DiffUpdate { get; set; }

        [DataMember(Name = "playlist")]
        public List<GoogleMusicSong> Songs { get; set; }

        [DataMember(Name = "continuation")]
        public bool Cont { get; set; }


        public string TrackString
        {
            get { return Songs.Count + " tracks"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }


    [DataContract]
    public class PreviewInfo
    {
        [DataMember(Name = "previewTrackId")]
        public String TrackID { get; set; }

        [DataMember(Name = "previewToken")]
        public String PreviewToken { get; set; }

        [DataMember(Name = "priceText")]
        public String Price { get; set; }

        [DataMember(Name = "purcahseUrl")]
        public String PurchaseURL { get; set; }
    }

    [DataContract]
    public class SharedSongList
    {
        [DataMember(Name = "shares")]
        public List<GoogleMusicSong> Shares { get; set; }
    }

    [DataContract]
    public class ShareInfo
    {
        [DataMember(Name = "postId")]
        public String PostID { get; set; }

        [DataMember(Name = "postText")]
        public String PostText { get; set; }

        [DataMember(Name = "previewListened")]
        public bool HasListened { get; set; }

        [DataMember(Name = "sharedByName")]
        public String SharedByName { get; set; }

        [DataMember(Name = "sharedByPhoto")]
        public String SharedByPhoto { get; set; }

        [DataMember(Name = "sharedByProfileUrl")]
        public String SharedByProfile { get; set; }

    }


    [DataContract]
    public class GoogleMusicSearchResults
    {
        [DataMember(Name = "albums")]
        public List<GoogleMusicSong> Albums { get; set; } // Not really a song. fix later

        [DataMember(Name = "artists")]
        public List<GoogleMusicSong> Artists { get; set; }

        [DataMember(Name = "songs")]
        public List<GoogleMusicSong> Songs { get; set; } // Not really a song. fix later
    }

    [DataContract]
    public class GoogleMusicSearch
    {
        [DataMember(Name = "results")]
        public GoogleMusicSearchResults Results { get; set; }
    }

    [DataContract]
    public class GoogleMusicStatus
    {
        [DataMember(Name = "totalTracks")]
        public int TotalTracks { get; set; }

        [DataMember(Name = "availableTracks")]
        public int AvailableTracks { get; set; }
    }

    [DataContract]
    public class Session
    {
        [DataMember(Name = "AuthToken")]
        public String AuthToken { get; set; }

        [DataMember(Name = "Cookies")]
        public List<Cookie> Cookies { get; set; }
    }
}
