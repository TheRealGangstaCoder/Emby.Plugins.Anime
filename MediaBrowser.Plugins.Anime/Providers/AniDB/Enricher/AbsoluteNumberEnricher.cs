using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Enricher
{
    public class AbsoluteNumberEnricher
    {
        private IApplicationPaths _applicationPaths;
        public AbsoluteNumberEnricher(IApplicationPaths paths)
        {
            _applicationPaths = paths;
        }

        public void EnrichWithEpisodeAbsoluteNumber(EpisodeInfo episodeInfo, Episode episode)
        {
            if (episodeInfo.SeriesProviderIds.ContainsKey("Tvdb"))
            {
                var tvdbId = episodeInfo.SeriesProviderIds
                    .Where(x => x.Key == "Tvdb")
                    .Select(y => y.Value).First();

                var episodeXml = GetEpisodeXmlFile(episodeInfo.ParentIndexNumber.Value, episodeInfo.IndexNumber.Value, tvdbId);

                XmlSerializer xs = new XmlSerializer(typeof(Tvdb.xsd.Episode));
                var episodeMetadata = xs.Deserialize(File.OpenRead(episodeXml.FullName)) as Tvdb.xsd.Episode;

                if (!string.IsNullOrEmpty(episodeMetadata.absolute_number) && episodeMetadata.absolute_number != "0")
                {
                    episode.Name = $"{episodeMetadata.absolute_number} - {episodeMetadata.EpisodeName} ";
                    episode.Overview = episodeMetadata.Overview;
                }
            }
        }

        private FileInfo GetEpisodeXmlFile(int seasonNumber, int episodeNumber, string tvdbid)
        {
            if (episodeNumber == null)
            {
                return null;
            }

            var seriesDataPath = Path.Combine(_applicationPaths.CachePath, "tvdb", tvdbid);

            const string nameFormat = "episode-{0}-{1}.xml";
            var filename = Path.Combine(seriesDataPath, string.Format(nameFormat, seasonNumber, episodeNumber));
            if (File.Exists(filename))
            {
                return new FileInfo(filename);
            }

            return null;
        }
    }
}
