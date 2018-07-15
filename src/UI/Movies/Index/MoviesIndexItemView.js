var vent = require('vent');
var Marionette = require('marionette');
var CommandController = require('../../Commands/CommandController');

module.exports = Marionette.ItemView.extend({
    ui : {
        refresh : '.x-refresh',
        search  : '.x-search'
    },

    events : {
        'click .x-edit'    : '_editMovie',
        'click .x-refresh' : '_refreshMovie',
        'click .x-search'  : '_searchMovie'
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name     : 'refreshMovie',
                movieId : this.model.get('id')
            }
        });

        CommandController.bindToCommand({
            element : this.ui.search,
            command : {
                name     : 'moviesSearch',
                movieIds : [this.model.get('id')]
            }
        });
    },

    _editMovie : function() {
        vent.trigger(vent.Commands.EditMovieCommand, { movie : this.model });
    },

    _refreshMovie : function() {
        CommandController.Execute('refreshMovie', {
            name     : 'refreshMovie',
            movieId : this.model.id
        });
    },

    _searchMovie : function() {
        CommandController.Execute('moviesSearch', {
            name     : 'moviesSearch',
            movieIds : [this.model.id]
        });
    }
});
