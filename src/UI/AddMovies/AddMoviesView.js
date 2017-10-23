var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var AddMoviesCollection = require('./AddMoviesCollection');
var AddFromListCollection = require('./List/AddFromListCollection');
var SearchResultCollectionView = require('./SearchResultCollectionView');
var DiscoverableListDropdownView = require("./DiscoverableListDropdownView");
var DiscoverableListCollection = require("./DiscoverableListCollection");
var DiscoverMoviesCollection = require("./DiscoverMoviesCollection");
var EmptyView = require('./EmptyView');
var NotFoundView = require('./NotFoundView');
var DiscoverEmptyView = require('./DiscoverEmptyView');
var ErrorView = require('./ErrorView');
var LoadingView = require('../Shared/LoadingView');
var FullMovieCollection = require("../Movies/FullMovieCollection");

module.exports = Marionette.Layout.extend({
		template : 'AddMovies/AddMoviesViewTemplate',

		regions : {
							myRegion : '#my-region',
				searchResult : '#search-result',
		},

		ui : {
				moviesSearch : '.x-movies-search',
				searchBar    : '.x-search-bar',
				loadMore     : '.x-load-more',
				discoverHeader : ".x-discover-header",
				discoverBefore : ".x-discover-before",
				discoverRecos : ".x-recommendations-tab",
				discoverPopular : ".x-popular-tab" ,
				discoverUpcoming : ".x-upcoming-tab",
				discoverLists : ".x-lists-tab"
		},

		events : {
				'click .x-load-more' : '_onLoadMore',
				"click .x-recommendations-tab" : "_discoverRecos",
				"click .x-popular-tab" : "_discoverPopular",
				"click .x-upcoming-tab" : "_discoverUpcoming",
				"click .x-lists-tab" : "_discoverLists",
				"click .discoverable-list-item" : "_discoverList"
		},

		initialize : function(options) {
				this.isExisting = options.isExisting;
				this.collection = options.collection || new AddMoviesCollection();

				if (this.isExisting) {
						this.collection.unmappedFolderModel = this.model;
				}

				if (this.isExisting) {
						this.className = 'existing-movies';
				} else {
						this.className = 'new-movies';
				}

				this.listenTo(vent, vent.Events.MoviesAdded, this._onMoviesAdded);
				this.listenTo(this.collection, 'sync', this._showResults);

				this.resultCollectionView = new SearchResultCollectionView({
						collection : this.collection,
						isExisting : this.isExisting
				});

				/*this.listsDropdown = new DiscoverableListCollectionView({
						collection : DiscoverableListCollection
				});*/

				this.listenTo(DiscoverableListCollection, 'sync', this._showListDropdown);
				/*this.listsDropdown = new DiscoverableListCollectionView({
						collection : DiscoverableListCollection
				})*/

				this.throttledSearch = _.debounce(this.search, 1000, { trailing : true }).bind(this);

				if (options.action === "search") {
					this.search({term: options.query});
				} else if (options.action === "discover") {
					this.isDiscover = true;
				}

		},

		onRender : function() {
				var self = this;



				this.$el.addClass(this.className);

				this.ui.moviesSearch.keyup(function(e) {

						if (_.contains([
										9,
										16,
										17,
										18,
										19,
										20,
										33,
										34,
										35,
										36,
										37,
										38,
										39,
										40,
										91,
										92,
										93
								], e.keyCode)) {
								return;
						}

						self._abortExistingSearch();
						self.throttledSearch({
								term : self.ui.moviesSearch.val()
						});
				});

				this._clearResults();

				if (this.isExisting) {
						this.ui.searchBar.hide();
				}

				if (this.isDiscover) {
						this.ui.searchBar.hide();
						this._discoverRecos();
						/*if (this.collection.length == 0) {
							this.searchResult.show(new LoadingView());
						}*/
				}
		},

		onShow : function() {
				this.ui.discoverBefore.hide();
				this.ui.moviesSearch.focus();
				this.ui.loadMore.hide();

				this._showListDropdown();

				if (this.isDiscover) {
						this.ui.discoverBefore.show();
				}
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
				if (!this.isDiscover) {
					this.ui.searchBar.show();
				}


				if (showingAll) {
						this.ui.loadMore.hide();
				}
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
								this.ui.loadMore.hide();
								if (this.isDiscover) {
									this.searchResult.show(new DiscoverEmptyView());
								} else {
									this.ui.searchBar.show();
									this.searchResult.show(new NotFoundView({ term : this.collection.term }));
								}

						} else {
								this.searchResult.show(this.resultCollectionView);
								if (!this.showingAll) {
										this.ui.loadMore.show();
								}
						}
				}
		},

		_showListDropdown : function() {
			this.listsDropdown = new DiscoverableListDropdownView(DiscoverableListCollection.toJSON());
			this.listsDropdown.render();
			$("#list-dropdown").html(this.listsDropdown.$el.html());
			//debugger;
			//this.myRegion.show(new DiscoverableListDropdownView(DiscoverableListCollection.toJSON()));
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
				if (!this.isClosed) {
						this.ui.searchBar.show();
						this.searchResult.show(new ErrorView({ term : this.collection.term }));
						this.collection.term = '';
				}
		},

		_discover : function(action) {
			if (this.collection.action === action) {
				return;
			}

			if (this.collection.specialProperty === "special") {
				this.collection.reset();
				this.collection = new DiscoverMoviesCollection();
				this.resultCollectionView.collection = this.collection;
			}

			this.listenTo(this.collection, 'sync', this._showResults);
			this.searchResult.show(new LoadingView());
			this.collection.action = action;
			this.currentSearchPromise = this.collection.fetch();
		},

		_discoverRecos : function() {
			this.ui.discoverRecos.tab("show");
			this.ui.discoverHeader.html("Recommendations by The Movie Database for you");
			this._discover("recommendations");
		},

		_discoverPopular : function() {
			this.ui.discoverPopular.tab("show");
			this.ui.discoverHeader.html("Currently Popular Movies");
			this._discover("popular");
		},

		_discoverUpcoming : function() {
			this.ui.discoverUpcoming.tab("show");
			this.ui.discoverHeader.html("Movies coming to Blu-Ray in the next weeks");
			this._discover("upcoming");
		},

		_discoverLists : function() {
			/*this.ui.discoverLists.tab("show");
			this.ui.discoverHeader.html("");*/
		},

		_discoverList : function(options) {
			this.ui.discoverLists.tab("show");
			this.ui.discoverHeader.html("Showing movies from list: "+options.target.textContent);

			this.collection.reset();
			this.collection = new AddFromListCollection();
			this.listenTo(this.collection, 'sync', this._showResults);
			this.searchResult.show(new LoadingView());
			this.currentSearchPromise = this.collection.fetch({ data: { listId: options.target.value } });
			this.resultCollectionView.collection = this.collection;
		}


});
