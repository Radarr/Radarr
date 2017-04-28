var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var ExistingMoviesCollectionView = require('./Existing/AddExistingMovieCollectionView');
var AddMoviesView = require('./AddMoviesView');
var ProfileCollection = require('../Profile/ProfileCollection');
var AddFromListView = require("./List/AddFromListView");
var RootFolderCollection = require('./RootFolders/RootFolderCollection');
var BulkImportView = require("./BulkImport/BulkImportView");
var DiscoverMoviesCollection = require("./DiscoverMoviesCollection");
require('../Movies/MoviesCollection');

module.exports = Marionette.Layout.extend({
		template : 'AddMovies/AddMoviesLayoutTemplate',

		regions : {
				workspace : '#add-movies-workspace',
		},

		ui : {
			$existing : '#show-existing-movies-toggle'
		},

		events : {
				'click .x-discover'  : '_discoverMovies',
				'click .x-bulk-import' : '_bulkImport',
				'click .x-add-new' : '_addMovies',
				"click .x-add-lists" : "_addFromList",
				'click .x-show-existing' : '_toggleExisting'
		},

		attributes : {
				id : 'add-movies-screen'
		},

		initialize : function(options) {
				ProfileCollection.fetch();
				RootFolderCollection.fetch().done(function() {
						RootFolderCollection.synced = true;
				});

				if (options.action === "search") {
					this._addMovies(options);
				}
		},

		_toggleExisting : function(e) {
			var showExisting = e.target.checked;

			vent.trigger(vent.Commands.ShowExistingCommand, {
					showExisting: showExisting
			});
		},

		onShow : function() {

				this.workspace.show(new AddMoviesView(this.options));
				this.ui.$existing.hide();
		},


		_folderSelected : function(options) {
				vent.trigger(vent.Commands.CloseModalCommand);
				//this.ui.$existing.show();
				this.workspace.show(new ExistingMoviesCollectionView({ model : options.model }));
		},

		_bulkFolderSelected : function(options) {
			vent.trigger(vent.Commands.CloseModalCommand);
			this.workspace.show(new BulkImportView({ model : options.model}));
		},

		_discoverMovies : function(options) {
			options = options || {};
			options.action = "discover";
			options.collection = new DiscoverMoviesCollection();
			this.workspace.show(new AddMoviesView(options));
		},

		_addMovies : function(options) {
				this.workspace.show(new AddMoviesView(options));
		},

		_addFromList : function() {
			//this.ui.$existing.hide();
			this.workspace.show(new AddFromListView());
		},

		_bulkImport : function() {
			this.bulkRootFolderLayout = new RootFolderLayout();
			this.listenTo(this.bulkRootFolderLayout, 'folderSelected', this._bulkFolderSelected);
			AppLayout.modalRegion.show(this.bulkRootFolderLayout);
		}
});
