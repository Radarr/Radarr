var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'file-browser-type-cell',

    render : function() {
        this.$el.empty();

        var type = this.model.get(this.column.get('name'));
        var icon = 'icon-lidarr-hdd';

        if (type === 'computer') {
            icon = 'icon-lidarr-browser-computer';
        } else if (type === 'parent') {
            icon = 'icon-lidarr-browser-up';
        } else if (type === 'folder') {
            icon = 'icon-lidarr-browser-folder';
        } else if (type === 'file') {
            icon = 'icon-lidarr-browser-file';
        }

        this.$el.html('<i class="{0}"></i>'.format(icon));
        this.delegateEvents();

        return this;
    }
});