var vent = require('vent');
var Marionette = require('marionette');
var moment = require('moment');

module.exports = Marionette.ItemView.extend({
    template : 'Calendar/UpcomingItemViewTemplate',
    tagName  : 'div',

    events : {
        'click .x-album-title' : '_showAlbumDetails'
    },

    initialize : function() {
        var start = this.model.get('releaseDate');
        var runtime = '30';
        var end = moment(start).add('minutes', runtime);

        this.model.set({
            end : end.toISOString()
        });

        this.listenTo(this.model, 'change', this.render);
    },

    _showAlbumDetails : function() {
        vent.trigger(vent.Commands.ShowAlbumDetails, { album : this.model });
    }
});