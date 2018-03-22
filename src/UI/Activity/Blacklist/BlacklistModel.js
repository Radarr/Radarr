var Backbone = require('backbone');
var MovieModel = require('../../Movies/MovieModel');
var MoviesCollection = require('../../Movies/FullMovieCollection');

module.exports = Backbone.Model.extend({
    parse : function(model) {

        //if (model.movie) {
        //    model.movie = new MovieModel(model.movie);
        //}

		model.movie = MoviesCollection.get(model.movieId);

        return model;
    }
});
