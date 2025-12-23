using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MediaItems
{
    public abstract class BaseMediaEditorController<TModel, TResource, TEditorResource> : Controller
        where TModel : ModelBase, new()
        where TResource : RestResource, new()
        where TEditorResource : class, IEditorResource
    {
        protected abstract List<TModel> GetItemsByIds(List<int> ids);
        protected abstract List<TModel> UpdateItems(List<TModel> items);
        protected abstract void DeleteItems(List<int> ids, bool deleteFiles);
        protected abstract ValidationResult ValidateItem(TModel item);
        protected abstract TResource ToResource(TModel model);

        protected abstract bool GetMonitored(TModel item);
        protected abstract void SetMonitored(TModel item, bool monitored);
        protected abstract int GetQualityProfileId(TModel item);
        protected abstract void SetQualityProfileId(TModel item, int qualityProfileId);
        protected abstract string GetRootFolderPath(TModel item);
        protected abstract void SetRootFolderPath(TModel item, string rootFolderPath);
        protected abstract HashSet<int> GetTags(TModel item);
        protected abstract void SetTags(TModel item, HashSet<int> tags);

        [HttpPut]
        public virtual IActionResult SaveAll([FromBody] TEditorResource resource)
        {
            var itemsToUpdate = GetItemsByIds(resource.Ids);

            foreach (var item in itemsToUpdate)
            {
                ApplyEditorChanges(item, resource);

                var validationResult = ValidateItem(item);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            var updatedItems = UpdateItems(itemsToUpdate);

            var resources = new List<TResource>(updatedItems.Count);
            foreach (var item in updatedItems)
            {
                resources.Add(ToResource(item));
            }

            return Ok(resources);
        }

        [HttpDelete]
        public virtual IActionResult DeleteAll([FromBody] TEditorResource resource)
        {
            DeleteItems(resource.Ids, resource.DeleteFiles);
            return Ok(new { });
        }

        protected virtual void ApplyEditorChanges(TModel item, TEditorResource resource)
        {
            if (resource.Monitored.HasValue)
            {
                SetMonitored(item, resource.Monitored.Value);
            }

            if (resource.QualityProfileId.HasValue)
            {
                SetQualityProfileId(item, resource.QualityProfileId.Value);
            }

            if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
            {
                SetRootFolderPath(item, resource.RootFolderPath);
            }

            if (resource.Tags != null)
            {
                ApplyTagChanges(item, resource.Tags, resource.ApplyTags);
            }
        }

        protected void ApplyTagChanges(TModel item, List<int> newTags, ApplyTags applyTags)
        {
            var currentTags = GetTags(item);

            switch (applyTags)
            {
                case ApplyTags.Add:
                    newTags.ForEach(t => currentTags.Add(t));
                    break;
                case ApplyTags.Remove:
                    newTags.ForEach(t => currentTags.Remove(t));
                    break;
                case ApplyTags.Replace:
                    SetTags(item, new HashSet<int>(newTags));
                    break;
            }
        }
    }
}
