var _ = require('underscore');
var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var MovieModel = require('./MovieModel');
var ApiData = require('../Shared/ApiData');
var AsFilteredCollection = require('../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../Mixins/AsPersistedStateCollection');
var moment = require('moment');
var UiSettings = require('../Shared/UiSettingsModel');
require('../Mixins/backbone.signalr.mixin');
var Config = require('../Config');

var pageSize = parseInt(Config.getValue("pageSize")) || 250;

var filterModes = {
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
    'unmonitored'  : [
        'monitored',
        false
    ],
    'missing'  : [
        'downloaded',
        false
    ],
    'released'  : [
        "status",
        "released",
        //function(model) { return model.getStatus() == "released"; }
    ],
    'announced'  : [
        "status",
        "announced",
        //function(model) { return model.getStatus() == "announced"; }
    ],
    'cinemas'  : [
        "status",
        "inCinemas",
        //function(model) { return model.getStatus() == "inCinemas"; }
    ]
}; //Hacky, I know


var Collection = PageableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/movie',
    model     : MovieModel,
    tableName : 'movie',

    origSetSorting : PageableCollection.prototype.setSorting,
    origAdd : PageableCollection.prototype.add,
    origSort : PageableCollection.prototype.sort,

    state : {
        sortKey            : 'sortTitle',
        order              : -1,
        pageSize           : pageSize,
        secondarySortKey   : 'sortTitle',
        secondarySortOrder : -1
    },

    queryParams : {
        totalPages   : null,
        totalRecords : null,
        pageSize     : 'pageSize',
        sortKey      : 'sortKey',
        order        : 'sortDir',
        directions   : {
            '-1' : 'asc',
            '1'  : 'desc'
        }
    },

    parseState : function(resp) {
	  if (this.mode === 'client') {
	  	return {};
	  }

	  if (this.state.pageSize === -1) {
	      return this.state;
      }

      var direction = -1;
      if (resp.sortDirection.toLowerCase() === "descending") {
        direction = 1;
      }
        return { totalRecords : resp.totalRecords, order : direction, currentPage : resp.page };
    },

    parseRecords : function(resp) {
        if (resp && this.mode !== 'client' && this.state.pageSize !== 0 && this.state.pageSize !== -1) {
            return resp.records;
        }

        return resp;
    },

    mode : 'server',

    setSorting : function(sortKey, order, options) {
        return this.origSetSorting.call(this, sortKey, order, options);
    },

    sort : function(options){
    	//if (this.mode == 'server' && this.state.order == '-1' && this.state.sortKey === 'sortTitle'){
        //    this.origSort(options);
        //}
    },

    save : function() {
        var self = this;
		var t= self;
		if (self.mode === 'client') {
			t = self.fullCollection;
			}
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
			if (self.mode === 'client') {
            	this.fullCollection.add(models, { merge : true });
			} else {
				this.add(models, { merge : true });
			}
            this.trigger('save', this);
        });

        return proxy.save();
    },

    importFromList : function(models) {
        var self = this;

        var proxy = _.extend(new Backbone.Model(), {
            id : "",

            url : self.url + "/import",

            toJSON : function() {
                return models;
            }
        });

        this.listenTo(proxy, "sync", function(proxyModel, models) {
            this.add(models, { merge : true});
            this.trigger("save", this);
        });

        return proxy.save();
    },

    filterModes : filterModes,

    sortMappings : {
        movie : {
            sortKey : 'movie.sortTitle'
        },
        title : {
            sortKey : 'sortTitle'
        },
        statusWeight : {
          sortValue : function(model, attr) {
            if (model.getStatus().toLowerCase() === "released") {
              return 3;
            }
            if (model.getStatus().toLowerCase() === "incinemas") {
              return 2;
            }
            if (model.getStatus().toLowerCase() === "announced") {
	      return 1;
	    }
            return -1;
          }
        },
        downloadedQuality : {
          sortValue : function(model, attr) {
            if (model.get("movieFile")) {
              return model.get("movieFile").quality.quality.name;
            }

            return "";
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
            if (model.get("downloaded")) {
              return -1;
            }
            return 0;
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
    },

    add : function(model, options) {
      if (this.length >= this.state.pageSize && this.state.pageSize !== -1) {
        return;
      }
      this.origAdd.call(this, model, options);
    },

    setFilterMode : function(mode){
      var arr = this.filterModes[mode];
      this.state.filterKey = arr[0];
      this.state.filterValue = arr[1];
      this.fetch();
    },

    comparator: function (model) {
		return model.get('sortTitle');
    }
});

Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);
Collection = AsPersistedStateCollection.call(Collection);

var filterMode = Config.getValue("movie.filterMode", "all");
var sortKey = Config.getValue("movie.sortKey", "sortTitle");
var sortDir = Config.getValue("movie.sortDirection", -1);
var sortD = "asc";
if (sortDir === 1) {
  sortD = "desc";
}

var values = filterModes[filterMode];

var data = ApiData.get("movie?page=1&pageSize={0}&sortKey={3}&sortDir={4}&filterKey={1}&filterValue={2}".format(pageSize, values[0], values[1], sortKey, sortD));

module.exports = new Collection(data.records, { full : false, state : { totalRecords : data.totalRecords} }).bindSignalR();
