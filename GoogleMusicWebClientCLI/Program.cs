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

        private void GetAllSongs(IGoogleMusicWebClient api)
        {
            var allSongs = api.GetAllSongs();
            allSongs.Wait();
            var songList = allSongs.Result.ToList();

            SaveSongsToFile(songList);
       }

        private static void SaveSongsToFile(List<GoogleMusicSong> songList)
        {
            var matched = songList.Where(p => p.Type == 6);
            var unmatched = songList.Where(p => p.Type != 6);

            WriteToFile(matched, "matched.txt");
            WriteToFile(unmatched, "unmatched.txt");
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
