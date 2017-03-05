var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var MoviesCollection = require('./MoviesCollection');
var FullMovieCollection = require("./FullMovieCollection");
var MoviesIndexLayout = require('./Index/MoviesIndexLayout');
var MoviesDetailsLayout = require('./Details/MoviesDetailsLayout');

module.exports = NzbDroneController.extend({
		_originalInit : NzbDroneController.prototype.initialize,

		initialize : function() {
				this.route('', this.series);
				this.route('movies', this.series);
				this.route('movies/:query', this.seriesDetails);

				this._originalInit.apply(this, arguments);
		},

		series : function() {
				this.setTitle('Movies');
				this.showMainRegion(new MoviesIndexLayout());
		},

		seriesDetails : function(query) {

			if(FullMovieCollection.length > 0) {
				this._renderMovieDetails(query);
			} else {
				this.listenTo(FullMovieCollection, 'sync', function(model, options) {
					this._renderMovieDetails(query);
				});
			}
		},


		_renderMovieDetails: function(query) {
			var movies = FullMovieCollection.where({ titleSlug : query });
			if (movies.length !== 0) {
					var targetMovie = movies[0];

					this.setTitle(targetMovie.get('title'));
					this.showMainRegion(new MoviesDetailsLayout({ model : targetMovie }));
			} else {
					this.showNotFound();
			}
		}
});
