var PagableCollection = require("backbone.pageable");
var ReleaseModel = require("./ReleaseModel");
var AsSortedCollection = require("../Mixins/AsSortedCollection");

var Collection = PagableCollection.extend({
    url   : window.NzbDrone.ApiRoot + "/release",
    model : ReleaseModel,

    state : {
        pageSize : 2000,
        sortKey  : "download",
        order    : -1
    },

    mode : "client",

    sortMappings : {
        "quality"    : {
            sortKey : "qualityWeight"
        },
        "rejections" : {
            sortValue : function(model) {
                var rejections = model.get("rejections");
                var releaseWeight = model.get("releaseWeight");

                if (rejections.length !== 0) {
                    return releaseWeight + 1000000;
                }

                return releaseWeight;
            }
        },
        "edition" : {
          sortKey : "edition"
        },
        "flags" : {
          sortValue : function(model) {
            var flags = model.get("indexerFlags");
            var weight = 0;
            if (flags) {
              _.each(flags, function(flag){
                var addon = "";
                var title = "";

                switch (flag) {
                  case "G_Halfleech":
                  weight += 1;
                  break;
                  case "G_Freeleech":
                  case "G_DoubleUpload":
                  case "PTP_Approved":
                  case "PTP_Golden":
                  case "HDB_Internal":
                  weight += 2;
                  break;
                }
              });
            }

            return weight;
          }
        },
        "download"   : {
            sortKey : "releaseWeight"
        },
        "seeders"    : {
            sortValue : function(model) {
                var seeders = model.get("seeders") || 0;
                var leechers = model.get("leechers") || 0;

                return seeders * 1000000 + leechers;
            }
        },
        "age"        : {
            sortKey : "ageMinutes"
        }
    },

    fetchEpisodeReleases : function(episodeId) {
        return this.fetch({ data : { episodeId : episodeId } });
    },

    fetchMovieReleases : function(movieId) {
      return this.fetch({ data : { movieId : movieId}});
    }

});

Collection = AsSortedCollection.call(Collection);

module.exports = Collection;
