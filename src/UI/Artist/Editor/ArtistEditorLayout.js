var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EmptyView = require('../Index/EmptyView');
var ArtistCollection = require('../ArtistCollection');
var ArtistTitleCell = require('../../Cells/ArtistTitleCell');
var ProfileCell = require('../../Cells/ProfileCell');
var ArtistStatusCell = require('../../Cells/ArtistStatusCell');
var AlbumFolderCell = require('../../Cells/AlbumFolderCell');
var SelectAllCell = require('../../Cells/SelectAllCell');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./ArtistEditorFooterView');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Artist/Editor/ArtistEditorLayoutTemplate',

    regions : {
        artistRegion : '#x-artist-editor',
        toolbar      : '#x-toolbar'
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
            name  : 'statusWeight',
            label : '',
            cell  : ArtistStatusCell
        },
        {
            name      : 'name',
            label     : 'Artist',
            cell      : ArtistTitleCell,
            cellValue : 'this'
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'albumFolder',
            label : 'Album Folder',
            cell  : AlbumFolderCell
        },
        {
            name  : 'path',
            label : 'Path',
            cell  : 'string'
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        items      : [
            {
                title : 'Album Studio',
                icon  : 'icon-lidarr-monitored',
                route : 'albumstudio'
            },
            {
                title          : 'Update Library',
                icon           : 'icon-lidarr-refresh',
                command        : 'refreshartist',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
        ]
    },

    initialize : function() {
        this.artistCollection = ArtistCollection.clone();
        this.artistCollection.bindSignalR();

        this.listenTo(this.artistCollection, 'save', this.render);

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'artisteditor.filterMode',
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
        this._showToolbar();
        this._showTable();
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showTable : function() {
        if (this.artistCollection.shadowCollection.length === 0) {
            this.artistRegion.show(new EmptyView());
            this.toolbar.close();
            return;
        }

        this.columns[0].sortedCollection = this.artistCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.artistCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.artistRegion.show(this.editorGrid);
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
            collection : this.artistCollection
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.artistCollection.setFilterMode(mode);
    }
});