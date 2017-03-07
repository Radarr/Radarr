﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api
{
    public class PagingResource<TResource>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public string FilterKey { get; set; }
        public string FilterValue { get; set; }
        public string FilterType { get; set;  }
        public int TotalRecords { get; set; }
        public List<TResource> Records { get; set; }
    }

    public static class PagingResourceMapper
    {
        public static PagingSpec<TModel> MapToPagingSpec<TResource, TModel>(this PagingResource<TResource> pagingResource, string defaultSortKey = "Id", SortDirection defaultSortDirection = SortDirection.Ascending)
        {
            var pagingSpec = new PagingSpec<TModel>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection,
            };

            if (pagingResource.SortKey == null)
            {
                pagingSpec.SortKey = defaultSortKey;
                if(pagingResource.SortDirection == SortDirection.Default)
                {
                    pagingSpec.SortDirection = defaultSortDirection;
                }
            }

            return pagingSpec;
        }

		/*public static Expression<Func<TModel, object>> CreateFilterExpression<TModel>(string filterKey, string filterValue)
		{
			Type type = typeof(TModel);
			ParameterExpression parameterExpression = Expression.Parameter(type, "x");
			Expression expressionBody = parameterExpression;

			return expressionBody;
		}*/
    }
}
