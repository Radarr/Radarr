var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var MoviesCollection = require('./MoviesCollection');
var FullMovieCollection = require("./FullMovieCollection");
var MoviesIndexLayout = require('./Index/MoviesIndexLayout');
var MoviesDetailsLayout = require('./Details/MoviesDetailsLayout');
var $ = require('jquery');

module.exports = NzbDroneController.extend({
		_originalInit : NzbDroneController.prototype.initialize,

		initialize : function() {
				this.route('', this.movies);
				this.route('movies', this.movies);
				this.route('movies/:query', this.movieDetails);

				this._originalInit.apply(this, arguments);
		},

		movies : function() {
				this.setTitle('Movies');
				this.showMainRegion(new MoviesIndexLayout());
		},

		movieDetails : function(query) {

			if(FullMovieCollection.length > 0) {
				this._renderMovieDetails(query);
				//debugger;
			} else {
				var self = this;
				$.getJSON(window.NzbDrone.ApiRoot + '/movie/titleslug/'+query, { }, function(data) {
						FullMovieCollection.add(data);
						self._renderMovieDetails(query);
					});
				this.listenTo(FullMovieCollection, 'sync', function(model, options) {
					//debugger;
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
