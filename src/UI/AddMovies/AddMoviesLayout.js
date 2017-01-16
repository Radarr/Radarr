var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var ExistingMoviesCollectionView = require('./Existing/AddExistingMovieCollectionView');
var AddMoviesView = require('./AddMoviesView');
var ProfileCollection = require('../Profile/ProfileCollection');
var RootFolderCollection = require('./RootFolders/RootFolderCollection');
require('../Movies/MoviesCollection');

module.exports = Marionette.Layout.extend({
    template : 'AddMovies/AddMoviesLayoutTemplate',

    regions : {
        workspace : '#add-movies-workspace'
    },

    events : {
        'click .x-import'  : '_importMovies',
        'click .x-add-new' : '_addMovies',
        'click .x-show-existing' : '_toggleExisting'
    },

    attributes : {
        id : 'add-movies-screen'
    },

    initialize : function() {
        ProfileCollection.fetch();
        RootFolderCollection.fetch().done(function() {
            RootFolderCollection.synced = true;
        });
    },

    _toggleExisting : function(e) {
        var showExisting = e.target.checked;
        
        vent.trigger(vent.Commands.ShowExistingCommand, {
            showExisting: showExisting
        });
    },

    onShow : function() {
        this.workspace.show(new AddMoviesView());
    },

    _folderSelected : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.workspace.show(new ExistingMoviesCollectionView({ model : options.model }));
    },

    _importMovies : function() {
        this.rootFolderLayout = new RootFolderLayout();
        this.listenTo(this.rootFolderLayout, 'folderSelected', this._folderSelected);
        AppLayout.modalRegion.show(this.rootFolderLayout);
    },

    _addMovies : function() {
        this.workspace.show(new AddMoviesView());
    }
});
