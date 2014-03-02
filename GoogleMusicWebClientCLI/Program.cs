using GoogleMusicWebClientAPI;
using System;
using System.Collections.Generic;
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
            //program.SearchSongs();
            program.GetAllSongs();

            //program.GetTrackCounts();

        }
        private static API GetAuthenticatedAPI()
        {
            var api = new API(new SessionStorage());

            Console.Write("User: ");
            var username = Console.ReadLine();
            Console.Write("Pass: ");
            var password = Console.ReadLine();

            var loginTask = api.Login(username, password);
            loginTask.Wait();
            var result = loginTask.Result;
            return api;
        }

        private void SearchSongs()
        {
            var api = GetAuthenticatedAPI();

            var searchTask = api.Search("candlemass");
            searchTask.Wait();
            var results = searchTask.Result;

        }

        private void GetAllSongs()
        {
            var api = GetAuthenticatedAPI();

            //var numberOfTracksTask = api.GetTrackCount();
            //numberOfTracksTask.Wait();
            //this.numberOfTracks = numberOfTracksTask.Result;

            var allSongs = api.GetAllSongs();
            allSongs.Wait();
            var songList = allSongs.Result.ToList();

            var matched = songList.Where(p => p.Type == 6);
            var unmatched = songList.Where(p => p.Type != 6);

            WriteToFile(matched, "matched.txt");
            WriteToFile(unmatched, "unmatched.txt");

            var firstSong = songList.First();

            var urlTask = api.GetStreamURL(firstSong);
            urlTask.Wait();
            var url = urlTask.Result;

            //foreach (var song in SongList)
            //{
            //    var songUrlTask = api.GetStreamURL(song);
            //    songUrlTask.Wait();

            //    var url = songUrlTask.Result;

            //    var filename = song.Artist + " " + song.Year + " " + song.Album + " " + song.Track + " " + song.Title + ".mp3";

            //    if (File.Exists(filename))
            //    {
            //        File.Delete(filename);
            //    }

            //    new WebClient().DownloadFile(url, filename);
            //}

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
