var NzbDroneCell = require('../../Cells/NzbDroneCell');
var BulkImportCollection = require('./BulkImportCollection');

module.exports = NzbDroneCell.extend({
    className : 'artist-title-cell',

    render : function() {
        var collection = this.model.collection;
        this.listenTo(collection, 'sync', this._renderCell);

        this._renderCell();

        return this;
    },

    _renderCell : function() {
        this.$el.empty();

        this.$el.html('<a href="https://www.musicbrainz.org/artist/' + this.cellValue.get('foreignArtistId') +'">' + this.cellValue.get('name') +'</a><br><span class="hint">' + this.cellValue.get('overview') + '</span>');
    }
});
