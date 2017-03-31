using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;
using Hqub.MusicBrainz.API.Entities.Metadata;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("recording", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Recording : Entity
    {
        public const string EntityName = "recording";

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
        /// Gets or sets the length.
        /// </summary>
        [XmlElement("length")]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the disambiguation.
        /// </summary>
        [XmlElement("disambiguation")]
        public string Disambiguation { get; set; }

        #endregion

        #region Include

        [XmlElement("tag-list")]
        public TagList Tags { get; set; }

        [XmlArray("artist-credit")]
        [XmlArrayItem("name-credit")]
        public List<NameCredit> Credits { get; set; }

        [XmlElement("release-list")]
        public ReleaseList Releases { get; set; }

        #endregion

        #region Static methods

        [Obsolete("Use GetAsync() method.")]
        public static Recording Get(string id, params string[] inc)
        {
            return GetAsync<Recording>(EntityName, id, inc).Result;
        }

        [Obsolete("Use SearchAsync() method.")]
        public static RecordingList Search(string query, int limit = 25, int offset = 0)
        {
            return SearchAsync<RecordingMetadata>(EntityName,
                query, limit, offset).Result.Collection;
        }

        [Obsolete("Use BrowseAsync() method.")]
        public static RecordingList Browse(string relatedEntity, string value, int limit = 25, int offset = 0, params  string[] inc)
        {
            return BrowseAsync<RecordingMetadata>(EntityName,
                relatedEntity, value, limit, offset, inc).Result.Collection;
        }

        /// <summary>
        /// Lookup an recording in the MusicBrainz database.
        /// </summary>
        /// <param name="id">The recording MusicBrainz id.</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public async static Task<Recording> GetAsync(string id, params string[] inc)
        {
            return await GetAsync<Recording>(EntityName, id, inc);
        }

        /// <summary>
        /// Search for an recording in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="limit">The maximum number of recordings to return (default = 25).</param>
        /// <param name="offset">The offset to the recordings list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<RecordingList> SearchAsync(string query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<RecordingMetadata>(EntityName,
                query, limit, offset)).Collection;
        }

        /// <summary>
        /// Search for an recording in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <param name="limit">The maximum number of recordings to return (default = 25).</param>
        /// <param name="offset">The offset to the recordings list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<RecordingList> SearchAsync(QueryParameters<Recording> query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<RecordingMetadata>(EntityName,
                query.ToString(), limit, offset)).Collection;
        }

        /// <summary>
        /// Browse all the recordings in the MusicBrainz database, which are directly linked to the
        /// entity with given id.
        /// </summary>
        /// <param name="entity">The name of the related entity.</param>
        /// <param name="id">The id of the related entity.</param>
        /// <param name="limit">The maximum number of recordings to return (default = 25).</param>
        /// <param name="offset">The offset to the recordings list (enables paging, default = 0).</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public async static Task<RecordingList> BrowseAsync(string entity, string id, int limit = 25, int offset = 0, params  string[] inc)
        {
            return (await BrowseAsync<RecordingMetadata>(EntityName,
                entity, id, limit, offset, inc)).Collection;
        }

        #endregion
    }
}
