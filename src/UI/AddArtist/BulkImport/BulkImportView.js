var $ = require('jquery');
var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ArtistNameCell = require('./BulkImportArtistNameCell');
var BulkImportCollection = require('./BulkImportCollection');
var ForeignIdCell = require('./ForeignIdCell');
var GridPager = require('../../Shared/Grid/Pager');
var SelectAllCell = require('./BulkImportSelectAllCell');
var ProfileCell = require('./BulkImportProfileCellT');
var MonitorCell = require('./BulkImportMonitorCell');
var ArtistPathCell = require('./ArtistPathCell');
var LoadingView = require('../../Shared/LoadingView');
var EmptyView = require('./EmptyView');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var CommandController = require('../../Commands/CommandController');
var Messenger = require('../../Shared/Messenger');
var ArtistCollection = require('../../Artist/ArtistCollection');
var ProfileCollection = require('../../Profile/ProfileCollection');

require('backgrid.selectall');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'AddArtist/BulkImport/BulkImportViewTemplate',

    regions : {
        toolbar : '#x-toolbar',
        table : '#x-artists-bulk',
    },

    ui : {
        addSelectdBtn : '.x-add-selected'
    },

    initialize : function(options) {
        ProfileCollection.fetch();
        this.bulkImportCollection = new BulkImportCollection().bindSignalR({ updateOnly : true });
        this.model = options.model;
        this.folder = this.model.get('path');
        this.folderId = this.model.get('id');
        this.bulkImportCollection.folderId = this.folderId;
        this.bulkImportCollection.folder = this.folder;
        this.bulkImportCollection.fetch();
        this.listenTo(this.bulkImportCollection, {'sync' : this._showContent, 'error' : this._showContent, 'backgrid:selected' : this._select});
    },

    columns : [
        {
            name : '',
            cell : SelectAllCell,
            headerCell : 'select-all',
            sortable : false,
            cellValue : 'this'
        },
        {
            name     : 'movie',
            label    : 'Artist',
            cell     : ArtistNameCell,
            cellValue : 'this',
            sortable : false
        },
        {
            name : 'path',
            label : 'Path',
            cell : ArtistPathCell,
            cellValue : 'this',
            sortable : false
        },
        {
            name	: 'foreignArtistId',
            label	: 'MB Id',
            cell	: ForeignIdCell,
            cellValue : 'this',
            sortable: false
        },
        {
            name :'monitor',
            label: 'Monitor',
            cell : MonitorCell,
            cellValue : 'this',
            sortable: false
        },
        {
            name : 'profileId',
            label : 'Profile',
            cell  : ProfileCell,
            cellValue : 'this',
            sortable: false
        }
    ],

    _showContent : function() {
        this._showToolbar();
        this._showTable();
    },

    onShow : function() {
        this.table.show(new LoadingView());
    },

    _showToolbar : function() {
        var leftSideButtons = {
            type : 'default',
            storeState: false,
            collapse : true,
            items : [
                {
                    title        : 'Add Selected',
                    icon         : 'icon-lidarr-add',
                    callback     : this._addSelected,
                    ownerContext : this,
                    className    : 'x-add-selected'
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            left    : [leftSideButtons],
            right   : [],
            context : this
        }));

        $('#x-toolbar').addClass('inline');
    },

    _addSelected : function() {
        var selected = _.filter(this.bulkImportCollection.models, function(elem){
            return elem.selected;
        });

        var promise = ArtistCollection.importFromList(selected);
        this.ui.addSelectdBtn.spinForPromise(promise);
        this.ui.addSelectdBtn.addClass('disabled');

        if (selected.length === 0) {
            Messenger.show({
                type    : 'error',
                message : 'No artists selected'
            });
            return;
        }

        Messenger.show({
            message : 'Importing {0} artists. This can take multiple minutes depending on how many artists should be imported. Don\'t close this browser window until it is finished!'.format(selected.length),
            hideOnNavigate : false,
            hideAfter : 30,
            type : 'error'
        });

        var _this = this;

        promise.done(function() {
            Messenger.show({
                message        : 'Imported artists from folder.',
                hideAfter      : 8,
                hideOnNavigate : true
            });


            _.forEach(selected, function(artist) {
                artist.destroy(); //update the collection without the added movies
            });
        });
    },

    _handleEvent : function(eventName, data) {
        if (eventName === 'sync' || eventName === 'content') {
            this._showContent();
        }
    },

    _select : function(model, selected) {
        model.selected = selected;
    },

    _showTable : function() {
        if (this.bulkImportCollection.length === 0) {
            this.table.show(new EmptyView({ folder : this.folder }));
            return;
        }

        this.importGrid = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.bulkImportCollection,
            className  : 'table table-hover'
        });

        this.table.show(this.importGrid);
    }
});
