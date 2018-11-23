using System;
using NzbDrone.Core.Datastore;
using Radarr.Http.REST;
using Radarr.Http.Validation;

namespace Radarr.Http
{
    public abstract class RadarrRestModule<TResource> : RestModule<TResource> where TResource : RestResource, new()
    {
        protected string Resource { get; private set; }


        private static string BaseUrl()
        {
            var isV3 = typeof(TResource).Namespace.Contains(".V2.");
            if (isV3)
            {
                return "/api/v2/";
            }
            return "/api/";
        }

        private static string ResourceName()
        {
            return new TResource().ResourceName.Trim('/').ToLower();
        }

        protected RadarrRestModule()
            : this(ResourceName())
        {
        }

        protected RadarrRestModule(string resource)
            : base(BaseUrl() + resource.Trim('/').ToLower())
        {
            Resource = resource;
            PostValidator.RuleFor(r => r.Id).IsZero();
            PutValidator.RuleFor(r => r.Id).ValidId();
        }

        protected PagingResource<TResource> ApplyToPage<TModel>(Func<PagingSpec<TModel>, PagingSpec<TModel>> function, PagingSpec<TModel> pagingSpec, Converter<TModel, TResource> mapper)
        {
            pagingSpec = function(pagingSpec);

            return new PagingResource<TResource>
            {
                Page = pagingSpec.Page,
                PageSize = pagingSpec.PageSize,
                SortDirection = pagingSpec.SortDirection,
                SortKey = pagingSpec.SortKey,
                TotalRecords = pagingSpec.TotalRecords,
                Records = pagingSpec.Records.ConvertAll(mapper)
            };
        }
    }
}
