using GoogleMusicWebClientAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicWebClientAPI.RecommendedForYou
{
    public class RecommendedForYouResponseProcessor
    {
        public IEnumerable<GoogleMusicSong> RecommendedForYouResponse(string playlist)
        {
            var trackList = new List<GoogleMusicSong>();

            dynamic jsonResponse = JsonConvert.DeserializeObject("{ stringArray : " + playlist + "}");

            var trackArray = jsonResponse.stringArray[1][0];

            foreach (var track in trackArray)
            {
                var song = GoogleMusicSong.BuildFromDynamic(track);
                trackList.Add(song);
            }

            return trackList;
        }

    }
}
