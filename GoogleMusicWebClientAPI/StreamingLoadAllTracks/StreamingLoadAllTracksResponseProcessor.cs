using GoogleMusicWebClientAPI;
using GoogleMusicWebClientAPI.Models;
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
                    var song = GoogleMusicSong.BuildFromDynamic(track);
                    trackList.Add(song);
                }

                match = match.NextMatch();
            }

            return trackList;
        }
    }
}
