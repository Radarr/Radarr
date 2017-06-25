var NzbDroneCell = require('../../Cells/NzbDroneCell');
var ArtistCollection = require('../ArtistCollection');

module.exports = NzbDroneCell.extend({
    className : 'track-rating-cell',


    render : function() {
        this.$el.empty();
        var ratings = this.model.get('ratings')

        if (ratings) {
            this.$el.html(ratings.value + ' (' + ratings.votes + ' votes)');
        }

        this.delegateEvents();
        return this;
    }
});