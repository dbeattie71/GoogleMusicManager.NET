using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI.Models
{
    public class GoogleMusicSongDiff
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string AlbumArt { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Composer { get; set; }
        public string Genre { get; set; }
        public long? Duration { get; set; }
        public int? Track { get; set; }
        public int? TotalTracks { get; set; }
        public int? Disc { get; set; }
        public int? TotalDiscs { get; set; }
        public int? Year { get; set; }
        public int? Playcount { get; set; }
        public float? CreationDate { get; set; }
        public double? LastPlayed { get; set; }
        public string StoreID { get; set; }
        public string MatchedID { get; set; }
        public int? Type { get; set; }
        public string Comment { get; set; }



        public string ArtURL { get; set; }
    }
}
