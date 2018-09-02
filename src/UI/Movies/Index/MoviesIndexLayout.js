var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var PosterCollectionView = require('./Posters/MoviePostersCollectionView');
var ListCollectionView = require('./Overview/MovieOverviewCollectionView');
var EmptyView = require('./EmptyView');
var MoviesCollection = require('../MoviesCollection');

var FullMovieCollection = require('../FullMovieCollection');
var InCinemasCell = require('../../Cells/InCinemasCell');

var RelativeDateCell = require('../../Cells/RelativeDateCell');

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
var Config = require('../../Config');

//var MoviesCollectionClient = require('../MoviesCollectionClient');


//this variable prevents double fetching the FullMovieCollection on first load
//var shownOnce = false;
//require('../Globals');
window.shownOnce = false;
module.exports = Marionette.Layout.extend({
    template : 'Movies/Index/MoviesIndexLayoutTemplate',

    regions : {
        moviesRegion : '#x-movies',
        toolbar      : '#x-toolbar',
        toolbar2     : '#x-toolbar2',
        footer       : '#x-movies-footer',
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
            name  : 'added',
            label : 'Date Added',
            cell  : RelativeDateCell
        },
        {
          name : "movieFile.quality",
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
            cell  : RelativeDateCell
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
                icon  : 'icon-radarr-add',
                route : 'addmovies'
            },
            {
                title : 'Movie Editor',
                icon  : 'icon-radarr-edit',
                route : 'movieeditor'
            },
            {
                title        : 'RSS Sync',
                icon         : 'icon-radarr-rss',
                command      : 'rsssync',
                errorMessage : 'RSS Sync Failed!'
            },
            {
              title : "PreDB Sync",
              icon : "icon-radarr-refresh",
              command : "predbsync",
              errorMessage : "PreDB Sync Failed!"
            },
            {
                title          : 'Update Library',
                icon           : 'icon-radarr-refresh',
                command        : 'refreshmovie',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
        ]
    },

    initialize : function() {
    	//this variable prevents us from showing the list before moviesCollection has been fetched the first time
        this.moviesCollection = MoviesCollection.clone();
        //debugger;
        this.moviesCollection.bindSignalR();
		var pageSize = parseInt(Config.getValue("pageSize")) || 10;
		if (this.moviesCollection.state.pageSize !== pageSize) {
        	this.moviesCollection.setPageSize(pageSize);
		}
        //this.listenTo(MoviesCollection, 'sync', function() {
		//	this.moviesCollection.fetch();
		//});

 		this.listenToOnce(this.moviesCollection, 'sync', function() {
            this._showToolbar();
            //this._fetchCollection();
            if (window.shownOnce) {
                //this._fetchCollection();
                this._showFooter();
            }
            window.shownOnce = true;
        });



	    this.listenTo(FullMovieCollection, 'sync', function() {
			this._showFooter();
		});

        /*this.listenTo(this.moviesCollection, 'sync', function(model, collection, options) {
            this._renderView();
			//MoviesCollectionClient.fetch();
        });*/
        this.listenTo(this.moviesCollection, "change", function(model) {
			if (model.get('saved'))	{
				model.set('saved', false);
				this.moviesCollection.fetch();
				//FullMovieCollection.fetch({reset : true });
				//this._showFooter();
				var m = FullMovieCollection.findWhere( { tmdbId : model.get('tmdbId') });
				m.set('monitored', model.get('monitored'));
				m.set('minimumAvailability', model.get('minimumAvailability'));
				m.set( {profileId : model.get('profileId') } );

				this._showFooter();
			}
		});


        this.listenTo(this.moviesCollection, 'remove', function(model, collection, options) {
			if (model.get('deleted')) {
				this.moviesCollection.fetch(); //need to do this so that the page shows a full page and the 'total records' number is updated
				//FullMovieCollection.fetch({reset : true}); //need to do this to update the footer
				FullMovieCollection.remove(model);
				this._showFooter();
			}

        });
		//this.moviesCollection.setPageSize(pageSize);


        this.sortingOptions = {
            type           : 'sorting',
            storeState     : false,
            viewCollection : this.moviesCollection,
            callback : this._sort,
            items          : [
                {
                    title : 'Title',
                    name  : 'title'
                },
                {
                    title: 'Downloaded',
                    name: 'movieFile.quality'
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
            menuKey       : 'movie.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-radarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-radarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'unmonitored',
                    title    : '',
                    tooltip  : 'UnMonitored Only',
                    icon     : 'icon-radarr-unmonitored',
                    callback : this._setFilter
                },
                {
                    key      : 'missing',
                    title    : '',
                    tooltip  : 'Missing Only',
                    icon     : 'icon-radarr-missing',
                    callback : this._setFilter
                },
                {
                    key      : 'released',
                    title    : '',
                    tooltip  : 'Released',
                    icon     : 'icon-radarr-movie-released',
                    callback : this._setFilter
                },
                {
                    key      : 'announced',
                    title    : '',
                    tooltip  : 'Announced',
                    icon     : 'icon-radarr-movie-announced',
                    callback : this._setFilter
                },
                {
                    key      : 'cinemas',
                    title    : '',
                    tooltip  : 'In Cinemas',
                    icon     : 'icon-radarr-movie-cinemas',
                    callback : this._setFilter
                }
            ]
        };

        this.viewButtons = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'moviesViewMode',
            defaultAction : 'listView',
            items         : [
                {
                    key      : 'posterView',
                    title    : '',
                    tooltip  : 'Posters',
                    icon     : 'icon-radarr-view-poster',
                    callback : this._showPosters
                },
                {
                    key      : 'listView',
                    title    : '',
                    tooltip  : 'Overview List',
                    icon     : 'icon-radarr-view-list',
                    callback : this._showList
                },
                {
                    key      : 'tableView',
                    title    : '',
                    tooltip  : 'Table',
                    icon     : 'icon-radarr-view-table',
                    callback : this._showTable
                }
            ]
        };

            //this._showToolbar();
            //debugger;
            var self = this;
            setTimeout(function(){self._showToolbar();}, 0); // jshint ignore:line
            //this._renderView();
    },

    onShow : function() {
/*		this.listenToOnce(this.moviesCollection, 'sync', function() {
        	this._showToolbar();
			//this._fetchCollection();
			if (window.shownOnce) {
				//this._fetchCollection();
				this._showFooter();
			}
			window.shownOnce = true;
		});
  */  },

    _showTable : function() {
        this.currentView = new Backgrid.Grid({
            collection : this.moviesCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        //this._showPager();
    	this._renderView();
    },

    _showList : function() {
        //this.current = "list";
        this.currentView = new ListCollectionView({
            collection : this.moviesCollection
        });

        this._renderView();
    },

    _showPosters : function() {
        this.currentView = new PosterCollectionView({
            collection : this.moviesCollection
        });

        this._renderView();
    },

    _sort : function() {
      console.warn("Sorting");
    },

    _renderView : function() {
        if (MoviesCollection.length === 0) {
            this.moviesRegion.show(new EmptyView());

            this.toolbar.close();
            this.toolbar2.close();
        } else {
            this.renderedOnce = true;
            this.moviesRegion.show(this.currentView);
			this.listenTo(this.currentView.collection, 'sync', function(eventName){
				this._showPager();
			});
            this._showToolbar();
        }
    },

	_fetchCollection : function() {
		this.moviesCollection.fetch();
	},

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');
        this.moviesCollection.setFilterMode(mode);
    },

    _showToolbar : function() {
      //debugger;
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
          collection : this.moviesCollection,
      });
      var pagerTop = new GridPager({
          columns    : this.columns,
          collection : this.moviesCollection,
      });
      this.pager.show(pager);
      this.pagerTop.show(pagerTop);
    },

    _showFooter : function() {
        var footerModel = new FooterModel();
        var movies = FullMovieCollection.models.length;
        //instead of all the counters could do something like this with different query in the where...
        //var releasedMovies = FullMovieCollection.where({ 'released' : this.model.get('released') });
        //    releasedMovies.length

        var announced = 0;
		var incinemas = 0;
		var released = 0;

    	var monitored = 0;

		var downloaded =0;
		var missingMonitored=0;
		var missingNotMonitored=0;
		var missingMonitoredNotAvailable=0;
		var missingMonitoredAvailable=0;

        var downloadedMonitored=0;
		var downloadedNotMonitored=0;

        _.each(FullMovieCollection.models, function(model) {

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
  			if (model.get('downloaded')) {
				downloadedMonitored++;
			}
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
   					if (model.get('monitored')) {
						missingMonitoredNotAvailable++;
					}
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
            movies      				    : movies,
            announced   				    : announced,
	    	incinemas   				    : incinemas,
	    	released     				    : released,
            monitored   				    : monitored,
            downloaded  				    : downloaded,
			downloadedMonitored			    : downloadedMonitored,
	    	downloadedNotMonitored 		    : downloadedNotMonitored,
	    	missingMonitored 			    : missingMonitored,
            missingMonitoredAvailable       : missingMonitoredAvailable,
	    	missingMonitoredNotAvailable    : missingMonitoredNotAvailable,
	    	missingNotMonitored 		    : missingNotMonitored
        });

        this.footer.show(new FooterView({ model : footerModel }));
    }
});
