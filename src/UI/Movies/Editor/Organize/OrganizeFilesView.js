var _ = require('underscore');
var vent = require('vent');
var Backbone = require('backbone');
var Marionette = require('marionette');
var CommandController = require('../../../Commands/CommandController');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Editor/Organize/OrganizeFilesViewTemplate',

    events : {
        'click .x-confirm-organize' : '_organize'
    },

    initialize : function(options) {
        this.movies = options.movies;
        this.templateHelpers = {
            numberOfMovies : this.movies.length,
            movies         : new Backbone.Collection(this.movies).toJSON()
        };
    },

    _organize : function() {
        var movieIds = _.pluck(this.movies, 'id');

        CommandController.Execute('renameMovie', {
            name      : 'renameMovie',
            movieIds : movieIds
        });

        this.trigger('organizingFiles');
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});