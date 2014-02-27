using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMusicManagerCLI
{
    class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path to scan.")]
        public string Path { get; set; }

        [Option('r', "recurse", Required = false, DefaultValue = false, HelpText = "Recurse directories.")]
        public bool Recurse { get; set; }

        [Option('o', "oauthfile", Required = false, DefaultValue = "oauth2.json", HelpText = "Oauth2 credentials file.")]
        public string OauthFile { get; set; }

        [Option('n', "noupload", Required = false, DefaultValue = false, HelpText = "Upload tracks if not matched.")]
        public bool NoUpload { get; set; }

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
