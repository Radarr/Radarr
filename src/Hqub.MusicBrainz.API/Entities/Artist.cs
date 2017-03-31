using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;
using Hqub.MusicBrainz.API.Entities.Metadata;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("artist", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Artist : Entity
    {
        public const string EntityName = "artist";

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
        /// Gets or sets the type.
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sort name.
        /// </summary>
        [XmlElement("sort-name")]
        public string SortName { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        [XmlElement("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the life-span.
        /// </summary>
        [XmlElement("life-span")]
        public LifeSpanNode LifeSpan { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [XmlElement("country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the disambiguation.
        /// </summary>
        [XmlElement("disambiguation")]
        public string Disambiguation { get; set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        [XmlElement("rating")]
        public Rating Rating { get; set; }

        #endregion

        #region Subqueries

        [XmlElement("recording-list")]
        public RecordingList Recordings { get; set; }

        [XmlElement("release-group-list")]
        public ReleaseGroupList ReleaseGroups { get; set; }

        [XmlElement("release-list")]
        public ReleaseList ReleaseLists { get; set; }

        [XmlElement("relation-list")]
        public RelationList RelationLists { get; set; }

        [XmlElement("work-list")]
        public WorkList Works { get; set; }

        [XmlElement("tag-list")]
        public TagList Tags { get; set; }

        #endregion

        #region Static Methods

        [Obsolete("Use GetAsync() method.")]
        public static Artist Get(string id, params string[] inc)
        {
            return GetAsync<Artist>(EntityName, id, inc).Result;
        }

        [Obsolete("Use SearchAsync() method.")]
        public static ArtistList Search(string query, int limit = 25, int offset = 0)
        {
            return SearchAsync<ArtistMetadata>(EntityName,
                query, limit, offset).Result.Collection;
        }

        [Obsolete("Use BrowseAsync() method.")]
        public static ArtistList Browse(string relatedEntity, string value, int limit = 25, int offset = 0, params  string[] inc)
        {
            return BrowseAsync<ArtistMetadata>(EntityName,
                relatedEntity, value, limit, offset, inc).Result.Collection;
        }

        /// <summary>
        /// Lookup an artist in the MusicBrainz database.
        /// </summary>
        /// <param name="id">The artist MusicBrainz id.</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public async static Task<Artist> GetAsync(string id, params string[] inc)
        {
            return await GetAsync<Artist>(EntityName, id, inc);
        }

        /// <summary>
        /// Search for an artist in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="limit">The maximum number of artists to return (default = 25).</param>
        /// <param name="offset">The offset to the artists list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<ArtistList> SearchAsync(string query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<ArtistMetadata>(EntityName,
                query, limit, offset)).Collection;
        }

        /// <summary>
        /// Search for an artist in the MusicBrainz database, matching the given query.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <param name="limit">The maximum number of artists to return (default = 25).</param>
        /// <param name="offset">The offset to the artists list (enables paging, default = 0).</param>
        /// <returns></returns>
        public async static Task<ArtistList> SearchAsync(QueryParameters<Artist> query, int limit = 25, int offset = 0)
        {
            return (await SearchAsync<ArtistMetadata>(EntityName,
                query.ToString(), limit, offset)).Collection;
        }

        /// <summary>
        /// Browse all the artists in the MusicBrainz database, which are directly linked to the
        /// entity with given id.
        /// </summary>
        /// <param name="entity">The name of the related entity.</param>
        /// <param name="id">The id of the related entity.</param>
        /// <param name="limit">The maximum number of artists to return (default = 25).</param>
        /// <param name="offset">The offset to the artists list (enables paging, default = 0).</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        public async static Task<ArtistList> BrowseAsync(string entity, string id, int limit = 25, int offset = 0, params  string[] inc)
        {
            return (await BrowseAsync<ArtistMetadata>(EntityName, entity, id,
                limit, offset, inc)).Collection;
        }

        #endregion
    }

    #region Include entities

    [XmlRoot("life-span", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class LifeSpanNode
    {
        /// <summary>
        /// Gets or sets the begin date.
        /// </summary>
        [XmlElement("begin")]
        public string Begin { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        [XmlElement("end")]
        public string End { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the life-span ended or not.
        /// </summary>
        [XmlElement("ended")]
        public bool Ended { get; set; }
    }

    #endregion

}
