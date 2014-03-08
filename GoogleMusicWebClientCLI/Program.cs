using GoogleMusicWebClientAPI;
using GoogleMusicWebClientAPI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleMusicWebClientCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program();
            var api = GetAuthenticatedAPI();
            program.GetAllSongs(api);
            program.GetRecommended(api);
            //program.GetTrackCounts();

        }
        private static IGoogleMusicWebClient GetAuthenticatedAPI()
        {
            var api = new GoogleMusicWebClient(new SessionStorage());

            Console.Write("User: ");
            var username = Console.ReadLine();
            Console.Write("Pass: ");
            var password = Console.ReadLine();

            var loginTask = api.Login(username, password);
            loginTask.Wait();
            var result = loginTask.Result;
            return api;
        }

        private void SearchSongs(IGoogleMusicWebClient api)
        {
            var searchTask = api.Search("candlemass");
            searchTask.Wait();
            var results = searchTask.Result;

        }

        private void GetAllSongs(IGoogleMusicWebClient api)
        {
            //var numberOfTracksTask = api.GetTrackCount();
            //numberOfTracksTask.Wait();
            //this.numberOfTracks = numberOfTracksTask.Result;


            var allSongs = api.GetAllSongs();
            allSongs.Wait();
            var songList = allSongs.Result.ToList();

            SaveSongsToFile(songList);

            var firstSong = songList.Where(p => p.Type == 6).First();

            var url = GetStreamUrl(api, firstSong);

            var exe = @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe";
            Process.Start(exe, url);

        }

        private static void SaveSongsToFile(List<GoogleMusicSong> songList)
        {
            var matched = songList.Where(p => p.Type == 6);
            var unmatched = songList.Where(p => p.Type != 6);

            WriteToFile(matched, "matched.txt");
            WriteToFile(unmatched, "unmatched.txt");
        }

        private string GetStreamUrl(IGoogleMusicWebClient api, GoogleMusicSong firstSong)
        {
            var urlTask = api.GetStreamURL(firstSong);
            urlTask.Wait();
            var url = urlTask.Result;
            return url;
        }

        private void GetRecommended(IGoogleMusicWebClient api)
        {
            var recommendedTask = api.GetRecommendedForYou();
            recommendedTask.Wait();
            var recommended = recommendedTask.Result;
            foreach (var track in recommended)
            {
                Console.WriteLine(track.Artist + ", " + track.Album + ", " + track.Track);
            }
        }

        private void GetTrackCounts()
        {
            var api = GetAuthenticatedAPI();
            var allSongs = api.GetTrackCount();
            allSongs.Wait();
            var x = allSongs.Result;
        }


        private static void WriteToFile(IEnumerable<GoogleMusicSong> matched, string title)
        {
            var stringList = matched.Select(line => line.Artist + "\t" + line.Year + "\t" + line.Album + "\t" + line.Track + "\t" + line.Title).OrderBy(p => p);

            using (var file = new StreamWriter(title))
            {
                foreach (var line in stringList)
                {
                    file.WriteLine(line);
                }
            }
        }


    }
}
