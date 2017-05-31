var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');
var CommandController = require('../Commands/CommandController');

module.exports = NzbDroneCell.extend({
    className : 'artist-actions-cell',

    ui : {
        refresh : '.x-refresh'
    },

    events : {
        'click .x-edit'    : '_editArtist',
        'click .x-refresh' : '_refreshArtist'
    },

    render : function() {
        this.$el.empty();

        this.$el.html('<i class="icon-lidarr-refresh x-refresh hidden-xs" title="" data-original-title="Update artist info and scan disk"></i> ' +
                      '<i class="icon-lidarr-edit x-edit" title="" data-original-title="Edit Artist"></i>');

        CommandController.bindToCommand({
            element : this.$el.find('.x-refresh'),
            command : {
                name     : 'refreshArtist',
                seriesId : this.model.get('id')
            }
        });

        this.delegateEvents();
        return this;
    },

    _editArtist : function() {
        vent.trigger(vent.Commands.EditArtistCommand, { artist : this.model });
    },

    _refreshArtist : function() {
        CommandController.Execute('refreshArtist', {
            name     : 'refreshArtist',
            artistId : this.model.id
        });
    }
});