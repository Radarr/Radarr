var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'artist-folder-cell',

    render : function() {
        this.$el.empty();
        var albumFolder = this.model.get(this.column.get('name'));
        this.$el.html(albumFolder.toString());

        return this;
    }
});