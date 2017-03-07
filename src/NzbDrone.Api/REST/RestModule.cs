﻿using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NzbDrone.Api.Extensions;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api.REST
{
    public abstract class RestModule<TResource> : NancyModule
        where TResource : RestResource, new()
    {
        private const string ROOT_ROUTE = "/";
        private const string ID_ROUTE = @"/(?<id>[\d]{1,10})";

        private Action<int> _deleteResource;
        private Func<int, TResource> _getResourceById;
        private Func<List<TResource>> _getResourceAll;
        private Func<PagingResource<TResource>, PagingResource<TResource>> _getResourcePaged;
        private Func<TResource> _getResourceSingle;
        private Func<TResource, int> _createResource;
        private Action<TResource> _updateResource;

        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }

        protected void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException(id + " is not a valid ID");
            }
        }

        protected RestModule(string modulePath)
            : base(modulePath)
        {
            ValidateModule();

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();
        }


        private void ValidateModule()
        {
            if (GetResourceById != null) return;

            if (CreateResource != null || UpdateResource != null)
            {
                throw new InvalidOperationException("GetResourceById route must be defined before defining Create/Update routes.");
            }
        }

        protected Action<int> DeleteResource
        {
            private get { return _deleteResource; }
            set
            {
                _deleteResource = value;
                Delete[ID_ROUTE] = options =>
                {
                    ValidateId(options.Id);
                    DeleteResource((int)options.Id);

                    return new object().AsResponse();
                };
            }
        }

        protected Func<int, TResource> GetResourceById
        {
            get { return _getResourceById; }
            set
            {
                _getResourceById = value;
                Get[ID_ROUTE] = options =>
                    {
                        ValidateId(options.Id);
                        try
                        {
                            var resource = GetResourceById((int)options.Id);

                            if (resource == null)
                            {
                                return new NotFoundResponse();
                            }

                            return resource.AsResponse();
                        }
                        catch (ModelNotFoundException)
                        {
                            return new NotFoundResponse();
                        }
                    };
            }
        }

        protected Func<List<TResource>> GetResourceAll
        {
            private get { return _getResourceAll; }
            set
            {
                _getResourceAll = value;

                Get[ROOT_ROUTE] = options =>
                {
                    var resource = GetResourceAll();
                    return resource.AsResponse();
                };
            }
        }

        protected Func<PagingResource<TResource>, PagingResource<TResource>> GetResourcePaged
        {
            private get { return _getResourcePaged; }
            set
            {
                _getResourcePaged = value;

                Get[ROOT_ROUTE] = options =>
                {
					var pagingSpec = ReadPagingResourceFromRequest();
					if (pagingSpec.Page == 0 && pagingSpec.PageSize == 0)
					{
						var all = GetResourceAll();
						return all.AsResponse();
					}
                    var resource = GetResourcePaged(pagingSpec);
                    return resource.AsResponse();
                };
            }
        }

        protected Func<TResource> GetResourceSingle
        {
            private get { return _getResourceSingle; }
            set
            {
                _getResourceSingle = value;

                Get[ROOT_ROUTE] = options =>
                {
                    var resource = GetResourceSingle();
                    return resource.AsResponse();
                };
            }
        }

        protected Func<TResource, int> CreateResource
        {
            private get { return _createResource; }
            set
            {
                _createResource = value;
                Post[ROOT_ROUTE] = options =>
                {
                    var id = CreateResource(ReadResourceFromRequest());
                    return GetResourceById(id).AsResponse(HttpStatusCode.Created);
                };

            }
        }

        protected Action<TResource> UpdateResource
        {
            private get { return _updateResource; }
            set
            {
                _updateResource = value;
                Put[ROOT_ROUTE] = options =>
                    {
                        var resource = ReadResourceFromRequest();
                        UpdateResource(resource);
                        return GetResourceById(resource.Id).AsResponse(HttpStatusCode.Accepted);
                    };

                Put[ID_ROUTE] = options =>
                    {
                        var resource = ReadResourceFromRequest();
                        resource.Id = options.Id;
                        UpdateResource(resource);
                        return GetResourceById(resource.Id).AsResponse(HttpStatusCode.Accepted);
                    };
            }
        }

        protected TResource ReadResourceFromRequest(bool skipValidate = false)
        {
            //TODO: handle when request is null
            var resource = Request.Body.FromJson<TResource>();

            if (resource == null)
            {
                throw new BadRequestException("Request body can't be empty");
            }

            var errors = SharedValidator.Validate(resource).Errors.ToList();

            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && !skipValidate && !Request.Url.Path.EndsWith("/test", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PostValidator.Validate(resource).Errors);
            }
            else if (Request.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PutValidator.Validate(resource).Errors);
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            return resource;
        }

        private PagingResource<TResource> ReadPagingResourceFromRequest()
        {
            int pageSize;
	      	int.TryParse(Request.Query.PageSize.ToString(), out pageSize);

            int page;
            int.TryParse(Request.Query.Page.ToString(), out page);


            var pagingResource = new PagingResource<TResource>
            {
                PageSize = pageSize,
                Page = page,
            };

            if (Request.Query.SortKey != null)
            {
                pagingResource.SortKey = Request.Query.SortKey.ToString();

                if (Request.Query.SortDir != null)
                {
                    pagingResource.SortDirection = Request.Query.SortDir.ToString()
                                                          .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }
            }

            if (Request.Query.FilterKey != null)
            {
                pagingResource.FilterKey = Request.Query.FilterKey.ToString();

                if (Request.Query.FilterValue != null)
                {
                    pagingResource.FilterValue = Request.Query.FilterValue.ToString();
                }

                if (Request.Query.FilterType != null)
                {
                    pagingResource.FilterType = Request.Query.FilterType.ToString();
                }
            }



            return pagingResource;
        }
    }
}
