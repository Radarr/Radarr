var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'backup-type-cell',

    render : function() {
        this.$el.empty();

        var icon = 'icon-radarr-backup-scheduled';
        var title = 'Scheduled';

        var type = this.model.get(this.column.get('name'));

        if (type === 'manual') {
            icon = 'icon-radarr-backup-manual';
            title = 'Manual';
        } else if (type === 'update') {
            icon = 'icon-radarr-backup-update';
            title = 'Before update';
        }

        this.$el.html('<i class="{0}" title="{1}"></i>'.format(icon, title));

        return this;
    }
});