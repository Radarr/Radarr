var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');
var CommandController = require('../Commands/CommandController');

module.exports = NzbDroneCell.extend({
    className : 'movie-actions-cell',

    ui : {
        refresh : '.x-refresh',
        search  : '.x-search'
    },

    events : {
        'click .x-edit'    : '_editMovie',
        'click .x-refresh' : '_refreshMovie',
        'click .x-search'  : '_searchMovie'
    },

    render : function() {
        this.$el.empty();

        this.$el.html('<i class="icon-radarr-refresh x-refresh hidden-xs" title="" data-original-title="Update movie info and scan disk"></i> ' +
                      '<i class="icon-radarr-edit x-edit" title="" data-original-title="Edit Movie"></i> ' +
                      '<i class="icon-radarr-search x-search" title="" data-original-title="Search Movie"></i>');

        CommandController.bindToCommand({
            element : this.$el.find('.x-refresh'),
            command : {
                name     : 'refreshMovie',
                movieId : this.model.get('id')
            }
        });

        CommandController.bindToCommand({
            element : this.$el.find('.x-search'),
            command : {
                name     : 'moviesSearch',
                movieIds : [this.model.get('id')]
            }
        });

        this.delegateEvents();
        return this;
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
