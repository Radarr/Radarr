var vent = require('vent');
var Marionette = require('marionette');
var CommandController = require('../../Commands/CommandController');

module.exports = Marionette.ItemView.extend({
    ui : {
        refresh : '.x-refresh'
    },

    events : {
        'click .x-edit'    : '_editArtist',
        'click .x-refresh' : '_refreshArtist'
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name     : 'refreshArtist',
                artistId : this.model.get('id')
            }
        });
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