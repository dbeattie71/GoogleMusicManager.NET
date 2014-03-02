using GoogleMusicWebClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI.StreamingLoadAllTracks
{
    public class StreamingLoadAllTracksResponseProcessor
    {
        public IEnumerable<GoogleMusicSong> ProcessStreamingLoadAllTracks(string playlist)
        {
            // var regex = new Regex(@"window.parent\['slat_process'\]\((?<tracks>.*?)\);\nwindow.parent\['slat_progress'\]\(1.0\);", RegexOptions.Singleline);
            var regex = new Regex(@"window.parent\['slat_process'\]\((?<tracks>.*?)\);\nwindow.parent\['slat_progress'\]", RegexOptions.Singleline);

            var match = regex.Match(playlist);

            var trackList = new List<GoogleMusicSong>();

            while (match.Success)
            {
                var tracks = match.Groups["tracks"].Value;

                dynamic jsonResponse = JsonConvert.DeserializeObject("{ stringArray : " + tracks + "}");

                var trackArray = jsonResponse.stringArray[0];

                foreach (var track in trackArray)
                {
                    var song = CreateTrack(track);
                    trackList.Add(song);
                }

                match = match.NextMatch();
            }

            return trackList;
        }

        private GoogleMusicSong CreateTrack(dynamic track)
        {
            // var stringvalue = track[6].ToString();
            var element = track[32];
            var condition = element != null; // string.IsNullOrEmpty(stringvalue) == false; 
            if (condition)
            {
                var trackString = (string)track.ToString();
                Debug.WriteLine(trackString);
            }

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
                LastPlayed = (float)track[25],
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
