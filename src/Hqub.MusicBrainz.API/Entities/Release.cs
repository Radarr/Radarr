using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;
using Hqub.MusicBrainz.API.Entities.Metadata;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("release", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Release : Entity
    {
        public const string EntityName = "release";

        #region Properties

        /// <summary>
        /// Gets or sets the score (only available in search results).
        /// </summary>
        [XmlAttribute("score", Namespace = "http://musicbrainz.org/ns/ext#-2.0")]
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the MusicBrainz id.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [XmlElement("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the quality.
        /// </summary>
        [XmlElement("quality")]
        public string Quality { get; set; }

        /// <summary>
        /// Gets or sets the text-representation.
        /// </summary>
        [XmlElement("text-representation")]
        public TextRepresentation TextRepresentation { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        [XmlElement("date")]
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [XmlElement("country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the barcode.
        /// </summary>
        [XmlElement("barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Gets or sets the release-group.
        /// </summary>
        [XmlElement("release-group")]
        public ReleaseGroup ReleaseGroup { get; set; }

        /// <summary>
        /// Gets or sets the cover-art-archive.
        /// </summary>
        [XmlElement("cover-art-archive")]
        public CoverArtArchive CoverArtArchive { get; set; }

        #endregion

        #region Subqueries

        [XmlArray("artist-credit")]
        [XmlArrayItem("name-credit")]
        public List<NameCredit> Credits { get; set; }

        [XmlArray("label-info-list")]
        [XmlArrayItem("label-info")]
        public List<LabelInfo> Labels { get; set; }

        [XmlElement("medium-list")]
        public Collections.MediumList MediumList { get; set; }

        #endregion

        #region Static Methods

        [Obsolete("Use GetAsync() method.")]
        public static Release Get(string id, params string[] inc)
        {
            return GetAsync<Release>(EntityName, id, inc).Result;
        }

        [Obsolete("Use SearchAsync() method.")]
        public static ReleaseList Search(string query, int limit = 25, int offset = 0)
        {
            return SearchAsync<ReleaseMetadata>(EntityName,
                query, limit, offset).Result.Collection;
        }

        /// <summary>
        /// Lookup a release in the MusicBrainz database.
        /// </summary>
        /// <param name="id">The release MusicBrainz id.</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public async static Task<Release> GetAsync(string id, params string[] inc)
        {
            return await GetAsync<Release>(EntityName, id, inc);
        }

        /// <summary>
        /// Search for a release in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="limit">The maximum number of releases to return (default = 25).</param>
        /// <param name="offset">The offset to the releases list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<ReleaseList> SearchAsync(string query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<ReleaseMetadata>(EntityName,
                query, limit, offset)).Collection;
        }

        /// <summary>
        /// Search for a release in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <param name="limit">The maximum number of releases to return (default = 25).</param>
        /// <param name="offset">The offset to the releases list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<ReleaseList> SearchAsync(QueryParameters<Release> query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<ReleaseMetadata>(EntityName,
                query.ToString(), limit, offset)).Collection;
        }

        /// <summary>
        /// Browse all the releases in the MusicBrainz database, which are directly linked to the
        /// entity with given id.
        /// </summary>
        /// <param name="entity">The name of the related entity.</param>
        /// <param name="id">The id of the related entity.</param>
        /// <param name="limit">The maximum number of releases to return (default = 25).</param>
        /// <param name="offset">The offset to the releases list (enables paging, default = 0).</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public static async Task<ReleaseList> BrowseAsync(string entity, string id, int limit = 25,
            int offset = 0, params string[] inc)
        {
            return (await BrowseAsync<ReleaseMetadata>(EntityName,
                entity, id, limit, offset, inc)).Collection;
        }

        #endregion
    }

    [XmlType(Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    [XmlRoot("text-representation", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class TextRepresentation
    {
        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [XmlElement("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        [XmlElement("script")]
        public string Script { get; set; }
    }
}
