using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class MediaServerUpdateQueue<TQueueHost, TItemInfo>
        where TQueueHost : class
    {
        private class UpdateQueue
        {
            public Dictionary<int, UpdateQueueItem<TItemInfo>> Pending { get; } = new ();
            public bool Refreshing { get; set; }
        }

        private readonly ICached<UpdateQueue> _pendingMoviesCache;

        public MediaServerUpdateQueue(ICacheManager cacheManager)
        {
            _pendingMoviesCache = cacheManager.GetRollingCache<UpdateQueue>(typeof(TQueueHost), "pendingMovies", TimeSpan.FromDays(1));
        }

        public void Add(string identifier, Movie movie, TItemInfo info)
        {
            var queue = _pendingMoviesCache.Get(identifier, () => new UpdateQueue());

            lock (queue)
            {
                var item = queue.Pending.TryGetValue(movie.Id, out var value)
                    ? value
                    : new UpdateQueueItem<TItemInfo>(movie);

                item.Info.Add(info);

                queue.Pending[movie.Id] = item;
            }
        }

        public void ProcessQueue(string identifier, Action<List<UpdateQueueItem<TItemInfo>>> update)
        {
            var queue = _pendingMoviesCache.Find(identifier);

            if (queue == null)
            {
                return;
            }

            lock (queue)
            {
                if (queue.Refreshing)
                {
                    return;
                }

                queue.Refreshing = true;
            }

            try
            {
                while (true)
                {
                    List<UpdateQueueItem<TItemInfo>> items;

                    lock (queue)
                    {
                        if (queue.Pending.Empty())
                        {
                            queue.Refreshing = false;
                            return;
                        }

                        items = queue.Pending.Values.ToList();
                        queue.Pending.Clear();
                    }

                    update(items);
                }
            }
            catch
            {
                lock (queue)
                {
                    queue.Refreshing = false;
                }

                throw;
            }
        }
    }

    public class UpdateQueueItem<TItemInfo>
    {
        public Movie Movie { get; set; }
        public HashSet<TItemInfo> Info { get; set; }

        public UpdateQueueItem(Movie movie)
        {
            Movie = movie;
            Info = new HashSet<TItemInfo>();
        }
    }
}
