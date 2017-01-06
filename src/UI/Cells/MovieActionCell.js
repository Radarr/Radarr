var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');
var CommandController = require('../Commands/CommandController');

module.exports = NzbDroneCell.extend({
    className : 'series-actions-cell',

    ui : {
        refresh : '.x-refresh'
    },

    events : {
        'click .x-edit'    : '_editSeries',
        'click .x-refresh' : '_refreshSeries'
    },

    render : function() {
        this.$el.empty();

        this.$el.html('<i class="icon-sonarr-refresh x-refresh hidden-xs" title="" data-original-title="Update movie info and scan disk"></i> ' +
                      '<i class="icon-sonarr-edit x-edit" title="" data-original-title="Edit Movie"></i>');

        CommandController.bindToCommand({
            element : this.$el.find('.x-refresh'),
            command : {
                name     : 'refreshMovie',
                movieId : this.model.get('id')
            }
        });

        this.delegateEvents();
        return this;
    },

    _editSeries : function() {
        vent.trigger(vent.Commands.EditSeriesCommand, { series : this.model });
    },

    _refreshSeries : function() {
        CommandController.Execute('refreshMovie', {
            name     : 'refreshMovie',
            movieId : this.model.id
        });
    }
});
