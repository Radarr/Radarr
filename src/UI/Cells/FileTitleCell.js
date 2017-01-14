var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'file-title-cell',

    render : function() {
        this.$el.empty();

        var title = this.model.get('relativePath');
            this.$el.html(title);


        return this;
    }
});
