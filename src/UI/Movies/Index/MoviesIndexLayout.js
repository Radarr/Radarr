var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var PosterCollectionView = require('./Posters/SeriesPostersCollectionView');
var ListCollectionView = require('./Overview/SeriesOverviewCollectionView');
var EmptyView = require('./EmptyView');
var MoviesCollection = require('../MoviesCollection');
var FullMovieCollection = require('../FullMovieCollection');
var InCinemasCell = require('../../Cells/InCinemasCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var TemplatedCell = require('../../Cells/TemplatedCell');
var ProfileCell = require('../../Cells/ProfileCell');
var MovieLinksCell = require('../../Cells/MovieLinksCell');
var MovieActionCell = require('../../Cells/MovieActionCell');
var MovieStatusCell = require('../../Cells/MovieStatusCell');
var MovieDownloadStatusCell = require('../../Cells/MovieDownloadStatusCell');
var DownloadedQualityCell = require('../../Cells/DownloadedQualityCell');
var FooterView = require('./FooterView');
var GridPager = require('../../Shared/Grid/Pager');
var FooterModel = require('./FooterModel');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Movies/Index/MoviesIndexLayoutTemplate',

    regions : {
        seriesRegion : '#x-series',
        toolbar      : '#x-toolbar',
        toolbar2     : '#x-toolbar2',
        footer       : '#x-series-footer',
        pager : "#x-movie-pager",
        pagerTop : "#x-movie-pager-top"
    },

    columns : [
        {
            name  : 'status',
            label : '',
            cell  : MovieStatusCell
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : MovieTitleCell,
            cellValue : 'this',
        },
        {
          name : "downloadedQuality",
          label : "Downloaded",
          cell : DownloadedQualityCell,
          sortable : true
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'inCinemas',
            label : 'In Cinemas',
            cell  : InCinemasCell
        },
        {
            name      : 'this',
            label     : 'Links',
            cell      : MovieLinksCell,
            className : "movie-links-cell",
            sortable : false,
        },
        {
          name        : "this",
          label       : "Status",
          cell        : MovieDownloadStatusCell,
          sortable : false,
          sortValue : function(m, k) {
            if (m.get("downloaded")) {
              return -1;
            }
            return 0;
          }
        },
        {
            name     : 'this',
            label    : '',
            sortable : false,
            cell     : MovieActionCell
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        collapse   : true,
        items      : [
            {
                title : 'Add Movie',
                icon  : 'icon-sonarr-add',
                route : 'addmovies'
            },
            {
                title : 'Movie Editor',
                icon  : 'icon-sonarr-edit',
                route : 'movieeditor'
            },
            {
                title        : 'RSS Sync',
                icon         : 'icon-sonarr-rss',
                command      : 'rsssync',
                errorMessage : 'RSS Sync Failed!'
            },
            {
                title          : 'Update Library',
                icon           : 'icon-sonarr-refresh',
                command        : 'refreshmovie',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
        ]
    },

    initialize : function() {
        this.seriesCollection = MoviesCollection.clone();
        this.seriesCollection.bindSignalR();
        this.fullCollection = FullMovieCollection;



        this.listenTo(this.seriesCollection, 'sync', function(model, collection, options) {
            //this.seriesCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.listenTo(MoviesCollection, "sync", function(eventName) {
          this.seriesCollection = MoviesCollection.clone();
          //this._showTable();
          this._renderView();
        });

        this.listenTo(this.seriesCollection, 'add', function(model, collection, options) {
            //this.seriesCollection.fullCollection.resetFiltered();
            //this._renderView();
        });

        this.listenTo(this.seriesCollection, 'remove', function(model, collection, options) {
            //this.seriesCollection.fullCollection.resetFiltered();
            //this._showTable();
        });

        this.sortingOptions = {
            type           : 'sorting',
            storeState     : false,
            viewCollection : this.seriesCollection,
            callback : this._sort,
            items          : [
                {
                    title : 'Title',
                    name  : 'title'
                },
                {
                    title: 'Downloaded',
                    name: 'downloadedQuality'
                },
                {
                    title : 'Profile',
                    name  : 'profileId'
                },
                {
                    title : 'In Cinemas',
                    name  : 'inCinemas'
                },
                /*{
                  title : "Status",
                  name : "status",
                }*/
            ]
        };

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'series.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'missing',
                    title    : '',
                    tooltip  : 'Missing Only',
                    icon     : 'icon-sonarr-missing',
                    callback : this._setFilter
                },
                {
                    key      : 'released',
                    title    : '',
                    tooltip  : 'Released',
                    icon     : 'icon-sonarr-movie-released',
                    callback : this._setFilter
                },
                {
                    key      : 'announced',
                    title    : '',
                    tooltip  : 'Announced',
                    icon     : 'icon-sonarr-movie-announced',
                    callback : this._setFilter
                },
                {
                    key      : 'cinemas',
                    title    : '',
                    tooltip  : 'In Cinemas',
                    icon     : 'icon-sonarr-movie-cinemas',
                    callback : this._setFilter
                }
            ]
        };

        this.viewButtons = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'seriesViewMode',
            defaultAction : 'listView',
            items         : [
                {
                    key      : 'posterView',
                    title    : '',
                    tooltip  : 'Posters',
                    icon     : 'icon-sonarr-view-poster',
                    callback : this._showPosters
                },
                {
                    key      : 'listView',
                    title    : '',
                    tooltip  : 'Overview List',
                    icon     : 'icon-sonarr-view-list',
                    callback : this._showList
                },
                {
                    key      : 'tableView',
                    title    : '',
                    tooltip  : 'Table',
                    icon     : 'icon-sonarr-view-table',
                    callback : this._showTable
                }
            ]
        };
    },

    onShow : function() {
        this._showToolbar();
        this._fetchCollection();
    },

    _showTable : function() {
        this.currentView = new Backgrid.Grid({
            collection : this.seriesCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this._showPager();

        this._renderView();
    },

    _showList : function() {
        //this.current = "list";
        this.currentView = new ListCollectionView({
            collection : this.seriesCollection
        });

        this._renderView();
    },

    _showPosters : function() {
        this.currentView = new PosterCollectionView({
            collection : this.seriesCollection
        });

        this._renderView();
    },

    _sort : function() {
      console.warn("Sorting");
    },

    _renderView : function() {
        if (MoviesCollection.length === 0) {
            this.seriesRegion.show(new EmptyView());

            this.toolbar.close();
            this.toolbar2.close();
        } else {

            this.seriesRegion.show(this.currentView);
            this.listenTo(this.currentView.collection, "sync", function(eventName){
              this._showPager();
              //debugger;
            });
            this._showToolbar();
            this._showFooter();
        }
    },

    _fetchCollection : function() {
        this.seriesCollection.fetch();
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');
        this.seriesCollection.setFilterMode(mode);
    },

    _showToolbar : function() {
        if (this.toolbar.currentView) {
            return;
        }

        this.toolbar2.show(new ToolbarLayout({
            right   : [
                this.filteringOptions
            ],
            context : this
        }));

        this.toolbar.show(new ToolbarLayout({
            right   : [
                this.sortingOptions,
                this.viewButtons
            ],
            left    : [
                this.leftSideButtons
            ],
            context : this
        }));
    },

    _showPager : function() {
      var pager = new GridPager({
          columns    : this.columns,
          collection : this.seriesCollection,
      });
      var pagerTop = new GridPager({
          columns    : this.columns,
          collection : this.seriesCollection,
      });
      this.pager.show(pager);
      this.pagerTop.show(pagerTop);
    },

    _showFooter : function() {
        var footerModel = new FooterModel();
        var movies = MoviesCollection.models.length;

        var announced = 0;
	var incinemas = 0;
	var released = 0;

    	var monitored = 0;

	var downloaded =0;
	var missingMonitored=0;
	var missingNotMonitored=0;
	var missingNotAvailable=0;
	var missingMonitoredAvailable=0;

	var downloadedNotMonitored=0;

        _.each(MoviesCollection.models, function(model) {

            if (model.get('status').toLowerCase() === 'released') {
                released++;
	    }
	    else if (model.get('status').toLowerCase() === 'incinemas') {
                incinemas++;
            }
	    else if (model.get('status').toLowerCase() === 'announced') {
                announced++;
            }

            if (model.get('monitored')) {
                monitored++;
	    }
	    else { //not monitored
		if (model.get('downloaded')) {
			downloadedNotMonitored++;
		}
		else { //missing
			missingNotMonitored++;
		}
	    }

	    if (model.get('downloaded')) {
		downloaded++;
	    }
            else { //missing
		if (!model.get('isAvailable')) {
			missingNotAvailable++;
		}

		if (model.get('monitored')) {
		    missingMonitored++;
		    if (model.get('isAvailable')) {
		        missingMonitoredAvailable++;
		    }
		}
            }
        });

        footerModel.set({
            movies      : movies,
            announced   : announced,
	    incinemas   : incinemas,
	    released     : released,
            monitored   : monitored,
            downloaded  : downloaded,
	    downloadedNotMonitored : downloadedNotMonitored,
	    missingMonitored : missingMonitored,
            missingMonitoredAvailable   : missingMonitoredAvailable,
	    missingNotAvailable : missingNotAvailable,
	    missingNotMonitored : missingNotMonitored
        });

        this.footer.show(new FooterView({ model : footerModel }));
    }
});
