var _ = require('underscore');
var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var MovieModel = require('./MovieModel');
var ApiData = require('../Shared/ApiData');
var AsFilteredCollection = require('../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../Mixins/AsPersistedStateCollection');
var moment = require('moment');
require('../Mixins/backbone.signalr.mixin');

var Collection = PageableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/movie',
    model     : MovieModel,
    tableName : 'movie',

    state : {
        sortKey            : 'sortTitle',
        order              : 1,
        pageSize           : 100000,
        secondarySortKey   : 'sortTitle',
        secondarySortOrder : -1
    },

    mode : 'client',

    save : function() {
        var self = this;

        var proxy = _.extend(new Backbone.Model(), {
            id : '',

            url : self.url + '/editor',

            toJSON : function() {
                return self.filter(function(model) {
                    return model.edited;
                });
            }
        });

        this.listenTo(proxy, 'sync', function(proxyModel, models) {
            this.add(models, { merge : true });
            this.trigger('save', this);
        });

        return proxy.save();
    },

    filterModes : {
        'all'        : [
            null,
            null
        ],
        'continuing' : [
            'status',
            'continuing'
        ],
        'ended'      : [
            'status',
            'ended'
        ],
        'monitored'  : [
            'monitored',
            true
        ],
        'missing'  : [
            'downloaded',
            false
        ]
    },

    sortMappings : {
        title : {
            sortKey : 'sortTitle'
        },
        statusWeight : {
          sortValue : function(model, attr) {
            if (model.getStatus() == "released") {
              return 1;
            }
            if (model.getStatus() == "inCinemas") {
              return 0;
            }
            return -1;
          }
        },
        downloadedQuality : {
          sortValue : function(model, attr) {
            if (model.get("movieFile")) {
              return 1000-model.get("movieFile").quality.quality.id;
            }

            return -1;
          }
        },
        nextAiring : {
            sortValue : function(model, attr, order) {
                var nextAiring = model.get(attr);

                if (nextAiring) {
                    return moment(nextAiring).unix();
                }

                if (order === 1) {
                    return 0;
                }

                return Number.MAX_VALUE;
            }
        },
        status: {
          sortValue : function(model, attr) {
            debugger;
            if (model.get("downloaded")) {
              return -1;
            }
            return 0;
          }
        },
        percentOfEpisodes : {
            sortValue : function(model, attr) {
                var percentOfEpisodes = model.get(attr);
                var episodeCount = model.get('episodeCount');

                return percentOfEpisodes + episodeCount / 1000000;
            }
        },
        inCinemas : {

          sortValue : function(model, attr) {
            var monthNames = ["January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
          ];
            if (model.get("inCinemas")) {
              return model.get("inCinemas");
            }
            return "2100-01-01";
          }
        },
        path : {
            sortValue : function(model) {
                var path = model.get('path');

                return path.toLowerCase();
            }
        }
    }
});

Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

var data = ApiData.get('movie');

module.exports = new Collection(data, { full : true }).bindSignalR();
