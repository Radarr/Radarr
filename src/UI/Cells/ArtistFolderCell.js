var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'artist-folder-cell',

    render : function() {
        this.$el.empty();

        var artistFolder = this.model.get(this.column.get('name'));
        this.$el.html(artistFolder.toString());

        return this;
    }
});