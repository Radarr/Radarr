var Backbone = require('backbone');
var MovieModel = require('../../Movies/MovieModel');

module.exports = Backbone.Model.extend({
    parse : function(model) {
        model.movie = new MovieModel(model.movie);
        return model;
    }
});
