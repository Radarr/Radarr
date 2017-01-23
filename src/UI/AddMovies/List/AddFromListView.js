var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var AddFromListCollection = require('./AddFromListCollection');
//var SearchResultCollectionView = require('./SearchResultCollectionView');
var AddListView = require("../../Settings/NetImport/Add/NetImportAddItemView");
var EmptyView = require('../EmptyView');
var NotFoundView = require('../NotFoundView');
var ListCollection = require("../../Settings/NetImport/NetImportCollection");
var ErrorView = require('../ErrorView');
var LoadingView = require('../../Shared/LoadingView');
var AppLayout = require('../../AppLayout');
var SchemaModal = require('../../Settings/NetImport/Add/NetImportSchemaModal');

module.exports = Marionette.Layout.extend({
		template : 'AddMovies/List/AddFromListViewTemplate',

		regions : {
				fetchResult : '#fetch-result'
		},

		ui : {
				moviesSearch : '.x-movies-search',
				listSelection : ".x-list-selection",

		},

		events : {
				'click .x-load-more' : '_onLoadMore',
				"change .x-list-selection" : "_listSelected",
				"click .x-fetch-list" : "_fetchList"
		},

		initialize : function(options) {
				console.log(options);

				this.isExisting = options.isExisting;
				//this.collection = new AddFromListCollection();

				this.templateHelpers = {}
				this.listCollection = new ListCollection();
				this.templateHelpers.lists = this.listCollection.toJSON();

				this.listenTo(this.listCollection, 'all', this._listsUpdated);
				this.listCollection.fetch();

				this.collection = new AddFromListCollection();

				/*this.listenTo(this.collection, 'sync', this._showResults);

				this.resultCollectionView = new SearchResultCollectionView({
						collection : this.collection,
						isExisting : this.isExisting
				});*/

				//this.throttledSearch = _.debounce(this.search, 1000, { trailing : true }).bind(this);
		},

		onRender : function() {
				var self = this;
		},

		onShow : function() {
				this.ui.moviesSearch.focus();
		},

		search : function(options) {
				var self = this;

				this.collection.reset();

				if (!options.term || options.term === this.collection.term) {
						return Marionette.$.Deferred().resolve();
				}

				this.searchResult.show(new LoadingView());
				this.collection.term = options.term;
				this.currentSearchPromise = this.collection.fetch({
						data : { term : options.term }
				});

				this.currentSearchPromise.fail(function() {
						self._showError();
				});

				return this.currentSearchPromise;
		},

		_onMoviesAdded : function(options) {
				if (this.isExisting && options.movie.get('path') === this.model.get('folder').path) {
						this.close();
				}

				else if (!this.isExisting) {
						this.resultCollectionView.setExisting(options.movie.get('tmdbId'));
						/*this.collection.term = '';
						this.collection.reset();
						this._clearResults();
						this.ui.moviesSearch.val('');
						this.ui.moviesSearch.focus();*/ //TODO: Maybe add option wheter to clear search result.
				}
		},

		_onLoadMore : function() {
				var showingAll = this.resultCollectionView.showMore();
				this.ui.searchBar.show();

				if (showingAll) {
						this.ui.loadMore.hide();
				}
		},

		_listSelected : function() {
			var rootFolderValue = this.ui.listSelection.val();
			if (rootFolderValue === 'addNew') {
					//var rootFolderLayout = new SchemaModal(this.listCollection);
					//AppLayout.modalRegion.show(rootFolderLayout);
					SchemaModal.open(this.listCollection)
			}
		},

		_fetchList : function() {
							var self = this;
			var listId = this.ui.listSelection.val();

			this.fetchResult.show(new LoadingView());

			this.currentFetchPromise = this.collection.fetch(
				{ data : { profileId : listId} }
			)
			this.currentFetchPromise.fail(function() {
					self._showError();
			});

		},

		_listsUpdated : function() {
			this.templateHelpers.lists = this.listCollection.toJSON();
			this.render();
		},

		_clearResults : function() {

				if (!this.isExisting) {
						this.searchResult.show(new EmptyView());
				} else {
						this.searchResult.close();
				}
		},

		_showResults : function() {
				if (!this.isClosed) {
						if (this.collection.length === 0) {
								this.ui.searchBar.show();
								this.searchResult.show(new NotFoundView({ term : this.collection.term }));
						} else {
								this.searchResult.show(this.resultCollectionView);
								if (!this.showingAll) {
										this.ui.loadMore.show();
								}
						}
				}
		},

		_abortExistingSearch : function() {
				if (this.currentSearchPromise && this.currentSearchPromise.readyState > 0 && this.currentSearchPromise.readyState < 4) {
						console.log('aborting previous pending search request.');
						this.currentSearchPromise.abort();
				} else {
						this._clearResults();
				}
		},

		_showError : function() {
						this.fetchResult.show(new ErrorView({ term : "" }));
		}
});
