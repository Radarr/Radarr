var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'track-explicit-cell',
    template  : 'Cells/TrackExplicitCellTemplate',

    render : function() {
        var explicit = this.cellValue.get('explicit');
        var print = '';

        if (explicit === true) {
            print = 'Explicit';
        }

        this.$el.html(print);
        return this;
    }

});