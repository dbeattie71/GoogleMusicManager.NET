using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI.Models
{
    public class GoogleMusicSong
    {
        string albumart;
        string albumArtist;
        string album;

        [DataMember(Name = "genre")]
        public string Genre { get; set; }

        [DataMember(Name = "beatsPerMinute")]
        public int BPM { get; set; }

        [DataMember(Name = "albumArtistNorm")]
        public string AlbumArtistNorm { get; set; }

        [DataMember(Name = "artistNorm")]
        public string ArtistNorm { get; set; }

        [DataMember(Name = "album")]
        public string Album
        {
            get { return album; }
            set
            {
                album = value;
            }
        }

        [DataMember(Name = "lastPlayed")]
        public double LastPlayed { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "disc")]
        public int Disc { get; set; }

        [DataMember(Name = "id")]
        public string ID { get; set; }

        [DataMember(Name = "composer")]
        public string Composer { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "albumArtist")]
        public string AlbumArtist
        {
            get { return albumArtist; }
            set
            {
                albumArtist = value;
            }
        }

        [DataMember(Name = "totalTracks")]
        public int TotalTracks { get; set; }

        public String AlbumDetailString
        {
            get
            {
                if (TotalTracks != 0 && !Genre.Equals(String.Empty))
                {
                    return String.Format("{0}, {1} tracks", Genre, TotalTracks);
                }
                else
                {
                    String r = "";
                    if (Genre != "")
                        r += Genre;
                    if (TotalTracks != 0 && Genre != "")
                        r += ", " + TotalTracks + " tracks";
                    if (TotalTracks != 0 && Genre == "")
                        return TotalTracks + " tracks";
                    return r;
                }
            }
        }
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "totalDiscs")]
        public int TotalDiscs { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }

        [DataMember(Name = "titleNorm")]
        public string TitleNorm { get; set; }

        [DataMember(Name = "artist")]
        public string Artist { get; set; }

        [DataMember(Name = "albumNorm")]
        public string AlbumNorm { get; set; }

        [DataMember(Name = "track")]
        public int Track { get; set; }

        [DataMember(Name = "durationMillis")]
        public long Duration { get; set; }

        public String DurationTimeSpan { get { return TimeSpan.FromMilliseconds(this.Duration).ToString("g"); } set { } }

        [DataMember(Name = "albumArt")]
        public string AlbumArt { get; set; }

        [DataMember(Name = "deleted")]
        public bool Deleted { get; set; }

        [DataMember(Name = "url")]
        public string URL { get; set; }

        [DataMember(Name = "creationDate")]
        public float CreationDate { get; set; }

        [DataMember(Name = "playCount")]
        public int Playcount { get; set; }

        [DataMember(Name = "rating")]
        public int Rating { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "matchedId")]
        public string MatchedID
        {
            get;
            set;
        }
        [DataMember(Name = "storeId")]
        public string StoreID
        {
            get;
            set;
        }

        [DataMember(Name = "albumArtUrl")]
        public string ArtURL
        {
            get
            {
                return (albumart != null && !albumart.StartsWith("http:")) ? "http:" + albumart : albumart;
            }
            set
            {
                albumart = value;

            }
        }

        [DataMember(Name = "previewInfo")]
        public PreviewInfo Preview { get; set; }

        [DataMember(Name = "sharingInfo")]
        public ShareInfo Share;

        public string ArtistAlbum
        {
            get
            {
                return Artist + ", " + Album;
            }
        }

        public TimeSpan DurationClean
        {
            get
            {
                return TimeSpan.FromMilliseconds(Duration);
            }
        }

        public static GoogleMusicSong BuildFromDynamic(dynamic track)
        {
            var gms = new GoogleMusicSong()
            {
                ID = track[0],
                Title = track[1],
                AlbumArt = track[2],
                Artist = track[3],
                Album = track[4],
                AlbumArtist = track[5],
                Composer = track[10],
                Genre = track[11],
                Duration = (long)track[13],
                Track = track[14] == null ? 0 : track[14],
                TotalTracks = track[15] == null ? 0 : track[15],
                Disc = track[16] == null ? 0 : track[16],
                TotalDiscs = track[17] == null ? 0 : track[17],
                Year = track[18] == null ? 0 : track[18],
                Playcount = track[22] == null ? 0 : track[22],
                CreationDate = (float)track[24],
                LastPlayed = track[25] == null ? 0 : (double)track[25],
                StoreID = track[27],
                MatchedID = track[28],
                // 27/28 seem same?
                Type = (int)track[29],
                Comment = track[30],
                // 31 fix match needed
                // 32 matched album
                // 33 matched artist
                // 34 bitrate
                // 35 recent timestamp
                ArtURL = track[36],
            };

            return gms;
        }


    }
}
