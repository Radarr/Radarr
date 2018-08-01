var $ = require('jquery');
var _ = require('underscore');
var vent = require('../../vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MissingCollection = require('./MissingCollection');
var SelectAllCell = require('../../Cells/SelectAllCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var MovieStatusWithTextCell = require('../../Cells/MovieStatusWithTextCell');
var GridPager = require('../../Shared/Grid/Pager');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var LoadingView = require('../../Shared/LoadingView');
var Messenger = require('../../Shared/Messenger');
var CommandController = require('../../Commands/CommandController');

require('backgrid.selectall');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Wanted/Missing/MissingLayoutTemplate',

    regions : {
        missing : '#x-missing',
        toolbar : '#x-toolbar',
        pager   : '#x-pager'
    },

    ui : {
        searchSelectedButton : '.btn i.icon-radarr-search'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : MovieTitleCell,
            cellValue : 'this',
        },
        {
            name  : 'inCinemas',
            label : 'In Cinemas',
            cell  : RelativeDateCell
        },
        {
            name  : 'physicalRelease',
            label : 'Physical Release',
            cell  : RelativeDateCell
        },
        {
            name     : 'status',
            label    : 'Status',
            cell     : MovieStatusWithTextCell,
            sortable : false
        },

    ],

    initialize : function() {
        this.collection = new MissingCollection().bindSignalR({ updateOnly : true });

        this.listenTo(this.collection, 'sync', this._showTable);
    },

    onShow : function() {
        this.missing.show(new LoadingView());
        this._showToolbar();
        this.collection.fetch();
    },

    _showTable : function() {
        this.missingGrid = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.collection,
            className  : 'table table-hover'
        });

        this.missing.show(this.missingGrid);

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : this.collection
        }));
    },

    _showToolbar    : function() {
        var leftSideButtons = {
            type       : 'default',
            storeState : false,
            collapse   : true,
            items      : [
                {
                    title        : 'Search Selected',
                    icon         : 'icon-radarr-search',
                    callback     : this._searchSelected,
                    ownerContext : this,
                    className    : 'x-search-selected'
                },
                {
                    title        : 'Search All',
                    icon         : 'icon-radarr-search',
                    callback     : this._searchMissing,
                    ownerContext : this,
                    className    : 'x-search-missing'
                },
                {
                    title        : 'Toggle Selected',
                    icon         : 'icon-radarr-monitored',
                    tooltip      : 'Toggle monitored status of selected',
                    callback     : this._toggleMonitoredOfSelected,
                    ownerContext : this,
                    className    : 'x-unmonitor-selected'
                },
                {
                    title      : 'Rescan Drone Factory Folder',
                    icon       : 'icon-radarr-refresh',
                    command    : 'downloadedMoviesScan',
                    properties : { sendUpdates : true }
                },
                {
                    title        : 'Manual Import',
                    icon         : 'icon-radarr-search-manual',
                    callback     : this._manualImport,
                    ownerContext : this
                }
            ]
        };

        var filterOptions = {
            type          : 'radio',
            storeState    : false,
            menuKey       : 'wanted.filterMode',
            defaultAction : 'monitored',
            items         : [
        		{
        	 	    key      : 'all',
        		    title    : '',
        		    tooltip  : 'All',
        		    icon     : 'icon-radarr-all',
        		    callback : this._setFilter
        		},
        		{
        	        key      : 'available',
        		    title    : '',
        		    tooltip  : 'Available & Monitored',
        		    icon     : 'icon-radarr-available',
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
                    tooltip  : 'Unmonitored Only',
                    icon     : 'icon-radarr-unmonitored',
                    callback : this._setFilter
                },
    		    {
    			    key      : 'announced',
    			    title    : '',
    			    tooltip  : 'Announced Only',
    			    icon     : 'icon-radarr-movie-announced',
    			    callback : this._setFilter
    		    },
    	            {     
    			    key      : 'incinemas',
    			    title    : '',
    			    tooltip  : 'In Cinemas Only',
    			    icon     : 'icon-radarr-movie-cinemas',
    			    callback : this._setFilter
    		    },
    		    {
    			    key      : 'released',
    			    title    : '',
    			    tooltip  : 'Released Only',
    			    icon     : 'icon-radarr-movie-released',
    			    callback : this._setFilter
    		    }
    		]
        };
        this.toolbar.show(new ToolbarLayout({
            left    : [leftSideButtons],
            right   : [filterOptions],
            context : this
        }));
        CommandController.bindToCommand({
            element : this.$('.x-search-selected'),
            command : { name : 'moviesSearch' }
        });
        CommandController.bindToCommand({
            element : this.$('.x-search-missing'),
            command : { name : 'missingMoviesSearch' }
        });
    },

    _setFilter      : function(buttonContext) {
        var mode = buttonContext.model.get('key');
        this.collection.state.currentPage = 1;
        var promise = this.collection.setFilterMode(mode);
        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _searchSelected : function() {
        var selected = this.missingGrid.getSelectedModels();
        if (selected.length === 0) {
            Messenger.show({
                type    : 'error',
                message : 'No movies selected'
            });
            return;
        }
        var ids = _.pluck(selected, 'id');
        CommandController.Execute('moviesSearch', {
            name       : 'moviesSearch',
            movieIds : ids
        });
    },
    _searchMissing  : function() {
        if (window.confirm('Are you sure you want to search for {0} filtered missing movies?'.format(this.collection.state.totalRecords) +
                           'One API request to each indexer will be used for each movie. ' + 'This cannot be stopped once started.')) {
            CommandController.Execute('missingMoviesSearch', { name : 'missingMoviesSearch',
	                                                       filterKey : this.collection.state.filterKey,
	   						       filterValue : this.collection.state.filterValue });
        }
    },
    _toggleMonitoredOfSelected : function() {
        var selected = this.missingGrid.getSelectedModels();

        if (selected.length === 0) {
            Messenger.show({
                type    : 'error',
                message : 'No movies selected'
            });
            return;
        }

        var promises = [];
        var self = this;

        _.each(selected, function (episode) {
            episode.set('monitored', !episode.get('monitored'));
            promises.push(episode.save());
        });

        $.when(promises).done(function () {
            self.collection.fetch();
        });
    },
    _manualImport : function () {
        vent.trigger(vent.Commands.ShowManualImport);
    }
});
