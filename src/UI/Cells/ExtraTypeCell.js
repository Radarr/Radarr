var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'extra-type-cell',

    render : function() {
        this.$el.empty();

        var title = this.model.get('type');
            this.$el.html(title);

        return this;
    }
});
