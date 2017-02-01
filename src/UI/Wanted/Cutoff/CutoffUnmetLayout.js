var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var CutoffUnmetCollection = require('./CutoffUnmetCollection');
var SelectAllCell = require('../../Cells/SelectAllCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var MovieStatusWithTextCell = require('../../Cells/MovieStatusWithTextCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var GridPager = require('../../Shared/Grid/Pager');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var LoadingView = require('../../Shared/LoadingView');
var Messenger = require('../../Shared/Messenger');
var CommandController = require('../../Commands/CommandController');

require('backgrid.selectall');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Wanted/Cutoff/CutoffUnmetLayoutTemplate',

    regions : {
        cutoff  : '#x-cutoff-unmet',
        toolbar : '#x-toolbar',
        pager   : '#x-pager'
    },

    ui : {
        searchSelectedButton : '.btn i.icon-sonarr-search'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name      : 'this',
            label     : 'Movie Title',
            cell      : MovieTitleCell,
            sortValue : false
        },
        {
            name      : 'inCinemas',
            label     : 'In Cinemas',
            cell      : RelativeDateCell
        },
        {
            name      : 'physicalRelease',
            label     : 'PhysicalRelease',
            cell      : RelativeDateCell
        },
        {
            name     : 'status',
            label    : 'Status',
            cell     : MovieStatusWithTextCell,
            sortable : false
        }
    ],

    initialize : function() {
        this.collection = new CutoffUnmetCollection().bindSignalR({ updateOnly : true });

        this.listenTo(this.collection, 'sync', this._showTable);
    },

    onShow : function() {
        this.cutoff.show(new LoadingView());
        this._showToolbar();
        this.collection.fetch();
    },

    _showTable : function() {
        this.cutoffGrid = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.collection,
            className  : 'table table-hover'
        });

        this.cutoff.show(this.cutoffGrid);

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : this.collection
        }));
    },

    _showToolbar : function() {
        var leftSideButtons = {
            type       : 'default',
            storeState : false,
            items      : [
                {
                    title        : 'Search Selected',
                    icon         : 'icon-sonarr-search',
                    callback     : this._searchSelected,
                    ownerContext : this,
                    className    : 'x-search-selected'
                },
                {
                    title        : 'Search All Missing',
                    icon         : 'icon-sonarr-search',
                    callback     : this._searchMissing,
                    ownerContext : this,
                    className    : 'x-search-missing'
                },
                {
                    title        : 'Toggle Selected',
                    icon         : 'icon-sonarr-monitored',
                    tooltip      : 'Toggle monitored status of selected',
                    callback     : this._toggleMonitoredOfSelected,
                    ownerContext : this,
                    className    : 'x-unmonitor-selected'
                },
                {
                    title : 'Season Pass',
                    icon  : 'icon-sonarr-monitored',
                    route : 'seasonpass'
                },
                {
                    title      : 'Rescan Drone Factory Folder',
                    icon       : 'icon-sonarr-refresh',
                    command    : 'downloadedMovieScan',
                    properties : { sendUpdates: true }
                },
                {
                    title        : 'Manual Import',
                    icon         : 'icon-sonarr-search-manual',
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
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'unmonitored',
                    title    : '',
                    tooltip  : 'Unmonitored Only',
                    icon     : 'icon-sonarr-unmonitored',
                    callback : this._setFilter
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            left    : [
                leftSideButtons
            ],
            right   : [
                filterOptions
            ],
            context : this
        }));

        CommandController.bindToCommand({
            element  : this.$('.x-search-selected'),
            command  : {
                name : 'moviesSearch'
            }
        });
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.collection.state.currentPage = 1;
        var promise = this.collection.setFilterMode(mode);

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _searchSelected : function() {
        var selected = this.cutoffGrid.getSelectedModels();

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
    }
});