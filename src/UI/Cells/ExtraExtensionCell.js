var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'extra-extension-cell',

    render : function() {
        this.$el.empty();

        var title = this.model.get('extension');
            this.$el.html(title);

        return this;
    }
});
