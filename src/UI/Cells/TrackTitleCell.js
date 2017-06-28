var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'track-title-cell',

    events : {
        //'click' : '_showDetails'
    },

    render : function() {
        var title = this.cellValue.get('title');

        if (!title || title === '') {
            title = 'TBA';
        }

        this.$el.html(title);
        return this;
    },

    _showDetails : function() {
        var hideArtistLink = this.column.get('hideArtistLink');
        //vent.trigger(vent.Commands.ShowTrackDetails, { //TODO Impelement Track search and screen as well as album?
        //    track        : this.cellValue,
        //    hideArtistLink : hideArtistLink
        //});
    }
});