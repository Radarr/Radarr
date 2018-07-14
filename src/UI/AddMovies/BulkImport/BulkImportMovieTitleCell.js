var NzbDroneCell = require('../../Cells/NzbDroneCell');
var BulkImportCollection = require("./BulkImportCollection");

module.exports = NzbDroneCell.extend({
		className : 'movie-title-cell',

        render : function() {
            var collection = this.model.collection;
            //this.listenTo(collection, 'sync', this._renderCell);

            this._renderCell();

            return this;
        },

        _renderCell : function() {
            this.$el.empty();

            this.$el.html('<a href="https://www.themoviedb.org/movie/' + this.cellValue.get('tmdbId') +'">' + this.cellValue.get('title') + ' (' + this.cellValue.get('year') + ')' +'</a>');
        }
});
