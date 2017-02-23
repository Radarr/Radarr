var Marionette = require('marionette');
var SearchResultView = require('./SearchResultView');
var MoviesCollection = require('../Movies/MoviesCollection');
var vent = require('vent');

module.exports = Marionette.CollectionView.extend({
		itemView : SearchResultView,

		initialize : function(options) {
				this.showExisting = true;
				this.isExisting = options.isExisting;
				this.showing = 5;
				if (this.isExisting) {
					this.showing = 1;
				}
				vent.on(vent.Commands.ShowExistingCommand, this._onExistingToggle.bind(this));
		},

		_onExistingToggle : function(data) {
				this.showExisting = data.showExisting;

				this.render();
		},

		showAll : function() {
				this.showingAll = true;
				this.render();
		},

		showMore : function() {
				this.showing += 5;
				this.render();

				return this.showing >= this.collection.length;
		},

		setExisting : function(tmdbid) {
				var movies = this.collection.where({ tmdbId : tmdbid });
				console.warn(movies);
				//debugger;
				if (movies.length > 0) {
						this.children.findByModel(movies[0])._configureTemplateHelpers();
						//this.children.findByModel(movies[0])._configureTemplateHelpers();
						this.children.findByModel(movies[0]).render();
						//this.templateHelpers.existing = existingMovies[0].toJSON();
				}
		},

		appendHtml : function(collectionView, itemView, index) {
				var tmdbId = itemView.model.get('tmdbId');
				var existingMovies = MoviesCollection.where({ tmdbId: tmdbId });
				if(existingMovies.length > 0) {
						if(this.showExisting) {
								if (index < this.showing || index === 0) {
										collectionView.$el.append(itemView.el);
								}
						}
				} else {
						if (index < this.showing || index === 0) {
								collectionView.$el.append(itemView.el);
						}
				}

		}
});
