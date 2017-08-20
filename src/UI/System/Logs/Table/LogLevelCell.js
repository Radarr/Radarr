var NzbDroneCell = require('../../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'log-level-cell',

    render : function() {
        var level = this._getValue();
        this.$el.html('<i class="icon-sonarr-log-{0}" title="{1}"></i>'.format(this._getValue().toLowerCase(), level));

        return this;
    }
});
