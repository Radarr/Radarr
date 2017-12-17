using System;
using System.Text;
using NzbDrone.Core.Indexers.Newznab;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class HeadphonesRssParser : NewznabRssParser
    {
        public const string ns = "{http://www.newznab.com/DTD/2010/feeds/attributes/}";
        public HeadphonesSettings Settings { get; set; }

        public HeadphonesRssParser()
        {
            PreferredEnclosureMimeTypes = UsenetEnclosureMimeTypes;
            UseEnclosureUrl = true;
        }

        protected override string GetBasicAuth()
        {
            return Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{Settings.Username}:{Settings.Password}")); ;
        }
    }
}
