using System;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API.Entities.Metadata;

namespace Hqub.MusicBrainz.API.Entities
{
    /// <summary>
    /// Base class for any entity returned by the MusicBrainz XML webservice.
    /// </summary>
    public abstract class Entity
    {
        private static string CreateIncludeQuery(string[] inc)
        {
            return string.Join("+", inc);
        }

        /// <summary>
        /// Sends a lookup request to the webservice.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="Entity"/>.</typeparam>
        /// <param name="entity">The name of the XML entity to lookup.</param>
        /// <param name="id">The MusicBrainz id of the entity.</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        protected async static Task<T> GetAsync<T>(string entity, string id, params string[] inc) where T : Entity
        {
            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentException(string.Format(Resources.Messages.MissingParameter, "entity"));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(string.Format(Resources.Messages.MissingParameter, "id"));
            }

            return await WebRequestHelper.GetAsync<T>(WebRequestHelper.CreateLookupUrl(entity, id, CreateIncludeQuery(inc)));
        }

        /// <summary>
        /// Sends a search request to the webservice.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="Entity"/>.</typeparam>
        /// <param name="entity">The name of the XML entity to search for.</param>
        /// <param name="query">The query string.</param>
        /// <param name="limit">The number of items to return (default = 25).</param>
        /// <param name="offset">The offset to the items list (enables paging, default = 0).</param>
        /// <returns></returns>
        protected async static Task<T> SearchAsync<T>(string entity, string query, int limit = 25, int offset = 0) where T : MetadataWrapper
        {
            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentException(string.Format(Resources.Messages.MissingParameter, "entity"));
            }

            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException(string.Format(Resources.Messages.MissingParameter, "query"));
            }

            return await WebRequestHelper.GetAsync<T>(WebRequestHelper.CreateSearchTemplate(entity,
                query, limit, offset), withoutMetadata: false);
        }
        
        /// <summary>
        /// Sends a browse request to the webservice.
        /// </summary>
        /// <typeparam name="T">Any type derived from <see cref="Entity"/>.</typeparam>
        /// <param name="entity">The name of the XML entity to browse.</param>
        /// <param name="relatedEntity"></param>
        /// <param name="relatedEntityId"></param>
        /// <param name="limit">The number of items to return (default = 25).</param>
        /// <param name="offset">The offset to the items list (enables paging, default = 0).</param>
        /// <param name="inc">A list of entities to include (subqueries).</param>
        /// <returns></returns>
        protected async static Task<T> BrowseAsync<T>(string entity, string relatedEntity, string relatedEntityId, int limit, int offset, params  string[] inc) where T : Entity
        {
            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentException(string.Format(Resources.Messages.MissingParameter, "entity"));
            }

            return await WebRequestHelper.GetAsync<T>(WebRequestHelper.CreateBrowseTemplate(entity,
                relatedEntity, relatedEntityId, limit, offset, CreateIncludeQuery(inc)), withoutMetadata: false);
        }
    }
}
