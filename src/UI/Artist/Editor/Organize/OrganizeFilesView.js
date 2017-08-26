var _ = require('underscore');
var vent = require('vent');
var Backbone = require('backbone');
var Marionette = require('marionette');
var CommandController = require('../../../Commands/CommandController');

module.exports = Marionette.ItemView.extend({
    template : 'Artist/Editor/Organize/OrganizeFilesViewTemplate',

    events : {
        'click .x-confirm-organize' : '_organize'
    },

    initialize : function(options) {
        this.artist = options.artist;
        this.templateHelpers = {
            numberOfArtists : this.artist.length,
            artist          : new Backbone.Collection(this.artist).toJSON()
        };
    },

    _organize : function() {
        var artistIds = _.pluck(this.artist, 'id');

        CommandController.Execute('renameArtist', {
            name      : 'renameArtist',
            artistIds : artistIds
        });

        this.trigger('organizingFiles');
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});