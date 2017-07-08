var _ = require('underscore');
var vent = require('vent');
var Backgrid = require('backgrid');
var Marionette = require('marionette');
var EmptyView = require('../Artist/Index/EmptyView');
var ArtistCollection = require('../Artist/ArtistCollection');
var ToolbarLayout = require('../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./AlbumStudioFooterView');
var SelectAllCell = require('../Cells/SelectAllCell');
var ArtistStatusCell = require('../Cells/ArtistStatusCell');
var ArtistTitleCell = require('../Cells/ArtistTitleCell');
var ArtistMonitoredCell = require('../Cells/ArtistMonitoredCell');
var AlbumsCell = require('./AlbumsCell');
require('../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'AlbumStudio/AlbumStudioLayoutTemplate',

    regions : {
        toolbar : '#x-toolbar',
        artist  : '#x-artist'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name  : 'statusWeight',
            label : '',
            cell  : ArtistStatusCell
        },
        {
            name       : 'monitored',
            label      : 'Artist',
            cell       : ArtistMonitoredCell,
            trueClass  : 'icon-lidarr-monitored',
            falseClass : 'icon-lidarr-unmonitored',
            tooltip    : 'Toggle artist monitored status',
            sortable   : false
        },

        {
            name      : 'albums',
            label     : 'Albums',
            cell      : AlbumsCell,
            cellValue : 'this'
        }
    ],

    initialize : function() {
        this.artistCollection = ArtistCollection.clone();

        this.artistCollection.shadowCollection.bindSignalR();

        this.listenTo(this.artistCollection, 'sync', this.render);
        this.listenTo(this.artistCollection, 'albumstudio:saved', this.render);

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'albumstudio.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-lidarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-lidarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'continuing',
                    title    : '',
                    tooltip  : 'Continuing Only',
                    icon     : 'icon-lidarr-artist-continuing',
                    callback : this._setFilter
                },
                {
                    key      : 'ended',
                    title    : '',
                    tooltip  : 'Ended Only',
                    icon     : 'icon-lidarr-artist-ended',
                    callback : this._setFilter
                }
            ]
        };
    },

    onRender : function() {
        this._showTable();
        this._showToolbar();
        this._showFooter();
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showToolbar : function() {
        this.toolbar.show(new ToolbarLayout({
            right   : [this.filteringOptions],
            context : this
        }));
    },

    _showTable : function() {
        if (this.artistCollection.shadowCollection.length === 0) {
            this.artist.show(new EmptyView());
            this.toolbar.close();
            return;
        }

        this.columns[0].sortedCollection = this.artistCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.artistCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.artist.show(this.editorGrid);
        this._showFooter();
    },

    _showFooter : function() {
        vent.trigger(vent.Commands.OpenControlPanelCommand, new FooterView({
            editorGrid : this.editorGrid,
            collection : this.artistCollection
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.artistCollection.setFilterMode(mode);
    }
});