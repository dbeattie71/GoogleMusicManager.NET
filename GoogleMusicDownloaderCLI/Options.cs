using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicDownloaderCLI
{
    class Options
    {
        [Option('o', "oauthfile", Required = false, DefaultValue = "oauth2.json", HelpText = "Oauth2 credentials file.")]
        public string OauthFile { get; set; }

        [Option('a', "artist", Required = false, DefaultValue = null, HelpText = "Artist filter.")]
        public string ArtistFilter { get; set; }

        [Option('l', "album", Required = false, DefaultValue = null, HelpText = "Album filter.")]
        public string AlbumFilter { get; set; }

        [Option('t', "track", Required = false, DefaultValue = null, HelpText = "Track filter.")]
        public string TrackFilter { get; set; }

        [Option('p', "path", Required = false, DefaultValue = "", HelpText = "Destination path.")]
        public string Path { get; set; }
        
        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
