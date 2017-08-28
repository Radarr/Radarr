var $ = require('jquery');
var _ = require('underscore');
var vent = require('../../vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MissingCollection = require('./MissingCollection');
var SelectAllCell = require('../../Cells/SelectAllCell');
var ArtistTitleCell = require('../../Cells/ArtistTitleCell');
var EpisodeNumberCell = require('../../Cells/EpisodeNumberCell');
var AlbumTitleCell = require('../../Cells/AlbumTitleCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var EpisodeStatusCell = require('../../Cells/EpisodeStatusCell');
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
        searchSelectedButton : '.btn i.icon-lidarr-search'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name      : 'artist',
            label     : 'Artist',
            cell      : ArtistTitleCell,
            sortValue : 'artist.sortName'
        },
//        {
//            name     : 'this',
//            label    : 'Ttack Number',
//            cell     : EpisodeNumberCell,
//            sortable : false
//        },
        {
            name     : 'this',
            label    : 'Album Title',
            cell     : AlbumTitleCell,
            sortable : false
        },
        {
            name  : 'releaseDate',
            label : 'Release Date',
            cell  : RelativeDateCell
        },
        {
            name     : 'status',
            label    : 'Status',
            cell     : EpisodeStatusCell,
            sortable : false
        }
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
                    icon         : 'icon-lidarr-search',
                    callback     : this._searchSelected,
                    ownerContext : this,
                    className    : 'x-search-selected'
                },
                {
                    title        : 'Search All Missing',
                    icon         : 'icon-lidarr-search',
                    callback     : this._searchMissing,
                    ownerContext : this,
                    className    : 'x-search-missing'
                },
                {
                    title        : 'Toggle Selected',
                    icon         : 'icon-lidarr-monitored',
                    tooltip      : 'Toggle monitored status of selected',
                    callback     : this._toggleMonitoredOfSelected,
                    ownerContext : this,
                    className    : 'x-unmonitor-selected'
                },
                {
                    title : 'Album Studio',
                    icon  : 'icon-lidarr-monitored',
                    route : 'albumstudio'
                },
                {
                    title      : 'Rescan Drone Factory Folder',
                    icon       : 'icon-lidarr-refresh',
                    command    : 'downloadedalbumsscan',
                    properties : { sendUpdates : true }
                },
                {
                    title        : 'Manual Import',
                    icon         : 'icon-lidarr-search-manual',
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
                    icon     : 'icon-lidarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'unmonitored',
                    title    : '',
                    tooltip  : 'Unmonitored Only',
                    icon     : 'icon-lidarr-unmonitored',
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
            command : { name : 'episodeSearch' }
        });
        CommandController.bindToCommand({
            element : this.$('.x-search-missing'),
            command : { name : 'missingEpisodeSearch' }
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
                message : 'No albums selected'
            });
            return;
        }
        var ids = _.pluck(selected, 'id');
        CommandController.Execute('episodeSearch', {
            name       : 'episodeSearch',
            episodeIds : ids
        });
    },
    _searchMissing  : function() {
        if (window.confirm('Are you sure you want to search for {0} missing albums? '.format(this.collection.state.totalRecords) +
                           'One API request to each indexer will be used for each album. ' + 'This cannot be stopped once started.')) {
            CommandController.Execute('missingEpisodeSearch', { name : 'missingEpisodeSearch' });
        }
    },
    _toggleMonitoredOfSelected : function() {
        var selected = this.missingGrid.getSelectedModels();

        if (selected.length === 0) {
            Messenger.show({
                type    : 'error',
                message : 'No albums selected'
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