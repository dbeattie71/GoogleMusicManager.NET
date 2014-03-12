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
        public string ID { get; set; }
        public string Title { get; set; }
        public string AlbumArt { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Composer { get; set; }
        public string Genre { get; set; }
        public long Duration { get; set; }
        public int Track { get; set; }
        public int TotalTracks { get; set; }
        public int Disc { get; set; }
        public int TotalDiscs { get; set; }
        public int Year { get; set; }
        public int Playcount { get; set; }
        public int Rating { get; set; }
        public float CreationDate { get; set; }
        public double LastPlayed { get; set; }
        public string StoreID { get; set; }
        public string MatchedID { get; set; }
        public int Type { get; set; }
        public string Comment { get; set; }



        public string ArtURL { get; set; }

        public int BPM { get; set; }
        public string AlbumArtistNorm { get; set; }
        public string ArtistNorm { get; set; }
        public string Name { get; set; }
        public string TitleNorm { get; set; }
        public string AlbumNorm { get; set; }
        public bool Deleted { get; set; }
        public string URL { get; set; }


        public PreviewInfo Preview { get; set; }
        public ShareInfo Share;

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
                Rating = track[23] == null ? 0 : track[23],
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

        public GoogleMusicSongDiff CompareTo(GoogleMusicSong update)
        {
            var diff = new GoogleMusicSongDiff()
            {
                ID = this.ID,
                Title = GetComparison(this.Title, update.Title),
                AlbumArt = GetComparison(this.AlbumArt, update.AlbumArt),
                Artist = GetComparison(this.Artist, update.Artist),
                Album = GetComparison(this.Album, update.Album),
                AlbumArtist = GetComparison(this.AlbumArtist, update.AlbumArtist),
                Composer = GetComparison(this.Composer, update.Composer),
                Genre = GetComparison(this.Genre, update.Genre),
                Duration = GetComparison(this.Duration, update.Duration),
                Track = GetComparison(this.Track, update.Track),
                TotalTracks = GetComparison(this.TotalTracks, update.TotalTracks),
                Disc = GetComparison(this.Disc, update.Disc),
                TotalDiscs = GetComparison(this.TotalDiscs, update.TotalDiscs),
                Year = GetComparison(this.Year, update.Year),
                Playcount = GetComparison(this.Playcount, update.Playcount),
                Rating = GetComparison(this.Rating, update.Rating),
                //CreationDate = GetComparison(this.CreationDate, update.CreationDate),
                //LastPlayed = GetComparison(this.LastPlayed, update.LastPlayed),
                StoreID = GetComparison(this.StoreID, update.StoreID),
                MatchedID = GetComparison(this.MatchedID, update.MatchedID),
                //Type = GetComparison(this.Type, update.Type),
                Comment = GetComparison(this.Comment, update.Comment),

                ArtURL = GetComparison(this.ArtURL, update.ArtURL),
            };
            return diff;
        }

        private T? GetComparison<T>(T from, T to) where T : struct, IComparable
        {
            if (from.CompareTo(to) == 0) return null;
            return to;
        }
        private string GetComparison(string from, string to) 
        {
            if (from == to) return null;
            return to;
        }
    }
}
