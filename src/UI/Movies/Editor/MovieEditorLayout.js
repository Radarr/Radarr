var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EmptyView = require('../Index/EmptyView');
var FullMovieCollection = require ('../FullMovieCollection');
var MoviesCollection = require('../MoviesCollection');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var MovieMonitoredCell = require('../../Cells/ToggleCell');
var DownloadedQualityCell = require('../../Cells/DownloadedQualityCell');
var ProfileCell = require('../../Cells/ProfileCell');
var SelectAllCell = require('../../Cells/SelectAllCell');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./MovieEditorFooterView');
var GridPager = require('../../Shared/Grid/Pager');
require('../../Mixins/backbone.signalr.mixin');
var DeleteSelectedView = require('./Delete/DeleteSelectedView');
var Config = require('../../Config');

window.shownOnce = false;
module.exports = Marionette.Layout.extend({
    template : 'Movies/Editor/MovieEditorLayoutTemplate',

    regions : {
        seriesRegion : '#x-series-editor',
        toolbar      : '#x-toolbar',
        pagerTop : "#x-movie-pager-top",
        pager : "#x-movie-pager"
    },

    ui : {
        monitored     : '.x-monitored',
        profiles      : '.x-profiles',
        rootFolder    : '.x-root-folder',
        selectedCount : '.x-selected-count'
    },

    events : {
        'click .x-save'         : '_updateAndSave',
        'change .x-root-folder' : '_rootFolderChanged'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name       : 'monitored',
            label      : '',
            cell       : MovieMonitoredCell,
            trueClass  : 'icon-sonarr-monitored',
            falseClass : 'icon-sonarr-unmonitored',
            tooltip    : 'Toggle movie monitored status',
            sortable   : false
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : MovieTitleCell,
            cellValue : 'this'
        },
        {
            name: "downloadedQuality",
            label: "Downloaded",
            cell: DownloadedQualityCell,
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'path',
            label : 'Path',
            cell  : 'string'
        }
    ],

    initialize : function() {

		this.movieCollection = MoviesCollection.clone();
		var pageSize = parseInt(Config.getValue("pageSize")) || 10;
		this.movieCollection.switchMode('client', {fetch: false});
		this.movieCollection.setPageSize(pageSize, {fetch: true});
        this.movieCollection.bindSignalR();
		this.movieCollection.fullCollection.bindSignalR();

		var selected = FullMovieCollection.where( { selected : true });
		_.each(selected, function(model) {
	     	model.set('selected', false);
		});

		this.listenTo(this.movieCollection, 'sync', function() {
			this._showToolbar();
			this._showTable();
			this._showPager();
			window.shownOnce = true;
		});

		this.listenTo(this.movieCollection.fullCollection, 'sync', function() {
			});


		this.leftSideButtons = {
            type       : 'default',
                storeState : false,
                collapse: true,
                items      : [
                {
                    title          : 'Update library',
                    icon           : 'icon-sonarr-refresh',
                    command        : 'refreshmovie',
                    successMessage : 'Library was updated!',
                    errorMessage   : 'Library update failed!'
                },
                {
                    title : 'Delete selected',
                    icon : 'icon-radarr-delete-white',
                    className: 'btn-danger',
                    callback : this._deleteSelected
                },
				{
                    title : 'Select All',
                    icon : 'icon-sonarr-checked',
                    className: 'btn-primary',
                    callback : this._selectAll
                },
				{
                    title : 'Unselect All',
                    icon : 'icon-sonarr-unchecked',
                    className: 'btn-primary',
                    callback : this._unselectAll
                }
            ]
        };
		//this.listenTo(FullMovieCollection, 'save', function() {
		//	window.alert('Done Saving');
		//});

        this.filteringOptions = {
            type          : 'radio',
            storeState    : false,
            menuKey       : 'serieseditor.filterMode',
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
                    key      : 'unmonitored',
                    title    : '',
                    tooltip  : 'UnMonitored Only',
                    icon     : 'icon-sonarr-unmonitored',
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
    },

    onRender : function() {
      	//this._showToolbar();
       	//this._showTable();
       	//this._showPager();
		    //if (window.shownOnce){
		    //	this.movieCollection.fetch();
		    //}
		    //window.shownOnce = true;
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showPager : function(){
      var pager = new GridPager({
          columns    : this.columns,
          collection : this.movieCollection
      });
      var pagerTop = new GridPager({
          columns    : this.columns,
          collection : this.movieCollection,
      });
      this.pager.show(pager);
      this.pagerTop.show(pagerTop);
    },

    _showTable : function() {
        if (this.movieCollection.length === 0) {
            this.seriesRegion.show(new EmptyView());
            this.toolbar.close();
            return;
        }
        this.columns[0].sortedCollection = this.movieCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.movieCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.seriesRegion.show(this.editorGrid);
       	this._showFooter();

    },

    _showToolbar : function() {
        this.toolbar.show(new ToolbarLayout({
            left    : [
                this.leftSideButtons
            ],
            right   : [
                this.filteringOptions
            ],
            context : this
        }));
    },

    _showFooter : function() {
        vent.trigger(vent.Commands.OpenControlPanelCommand, new FooterView({
            editorGrid : this.editorGrid,
            collection : this.movieCollection
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');
        this.movieCollection.setFilterMode(mode);
    },

    _deleteSelected: function() {
        var selected = FullMovieCollection.where({ selected : true });
        var updateFilesMoviesView = new DeleteSelectedView({ movies : selected });

        vent.trigger(vent.Commands.OpenModalCommand, updateFilesMoviesView);
    },
    
    _selectAll : function() {
		    var pageSize = this.movieCollection.state.pageSize;
		    var currentPage = this.movieCollection.state.currentPage;
		    this.movieCollection.setPageSize(this.movieCollection.fullCollection.length, { fetch: false});
		    this.movieCollection.each(function(model) {
		    	model.trigger('backgrid:selected', model, true);	
		    });
		    this.movieCollection.setPageSize(pageSize, {fetch: false});
		    this.movieCollection.getPage(currentPage, {fetch: false});
	},

	_unselectAll : function() {
		    var pageSize = this.movieCollection.state.pageSize;
		    var currentPage = this.movieCollection.state.currentPage;
		    this.movieCollection.setPageSize(this.movieCollection.fullCollection.length, { fetch: false});
		    this.movieCollection.each(function(model) {
		   	model.trigger('backgrid:selected', model, false);
	        });
		    this.movieCollection.setPageSize(pageSize, {fetch: false});
		    this.movieCollection.getPage(currentPage, {fetch: false});
	}
	
});
