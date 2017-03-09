var Backbone = require('backbone');
var SeriesModel = require('../../Series/SeriesModel');
var EpisodeModel = require('../../Series/EpisodeModel');
var MovieModel = require('../../Movies/MovieModel');
var MoviesCollection = require('../../Movies/FullMovieCollection');

module.exports = Backbone.Model.extend({
    parse : function(model) {
        if (model.series) {
          model.series = new SeriesModel(model.series);
          model.episode = new EpisodeModel(model.episode);
          model.episode.set('series', model.series);
        }

        //if (model.movie) {
        //    model.movie = new MovieModel(model.movie);
        //}

		model.movie = MoviesCollection.get(model.movieId);

        return model;
    }
});
