﻿using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tags
{
    public interface ITagRepository : IBasicRepository<Tag>
    {
        Tag GetByLabel(string label);
    }

    public class TagRepository : BasicRepository<Tag>, ITagRepository
    {
        public TagRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Tag GetByLabel(string label)
        {
            var model = Query(q => q.Where(c => c.Label == label).SingleOrDefault());

            if (model == null)
            {
                throw new InvalidOperationException("Didn't find tag with label " + label);
            }

            return model;
        }
    }
}
