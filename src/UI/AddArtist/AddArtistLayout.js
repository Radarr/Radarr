var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var ExistingArtistCollectionView = require('./Existing/AddExistingArtistCollectionView');
var AddArtistView = require('./AddArtistView');
var ProfileCollection = require('../Profile/ProfileCollection');
var RootFolderCollection = require('./RootFolders/RootFolderCollection');
var BulkImportView = require('./BulkImport/BulkImportView');
require('../Artist/ArtistCollection');

module.exports = Marionette.Layout.extend({
    template : 'AddArtist/AddArtistLayoutTemplate',

    regions : {
        workspace : '#add-artist-workspace'
    },

    events : {
        'click .x-import'  : '_importArtist',
        'click .x-bulk-import' : '_bulkImportArtist',
        'click .x-add-new' : '_addArtist'
    },

    attributes : {
        id : 'add-artist-screen'
    },

    initialize : function() {
        ProfileCollection.fetch();
        RootFolderCollection.fetch().done(function() {
            RootFolderCollection.synced = true;
        });
    },

    onShow : function() {
        this.workspace.show(new AddArtistView());
    },

    _folderSelected : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);

        this.workspace.show(new ExistingArtistCollectionView({ model : options.model }));
    },

    _bulkFolderSelected : function(options) {
           vent.trigger(vent.Commands.CloseModalCommand);
           this.workspace.show(new BulkImportView({ model : options.model}));
    },

    _importArtist : function() {
        this.rootFolderLayout = new RootFolderLayout();
        this.listenTo(this.rootFolderLayout, 'folderSelected', this._folderSelected);
        AppLayout.modalRegion.show(this.rootFolderLayout);
    },

    _addArtist : function() {
        this.workspace.show(new AddArtistView());
    },

    _bulkImportArtist : function() {
           this.bulkRootFolderLayout = new RootFolderLayout();
           this.listenTo(this.bulkRootFolderLayout, 'folderSelected', this._bulkFolderSelected);
           AppLayout.modalRegion.show(this.bulkRootFolderLayout);
    }
});