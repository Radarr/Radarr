var Marionette = require('marionette');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'queue-status-cell',
    template  : 'Activity/Queue/QueueStatusCellTemplate',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {
            var status = this.cellValue.get('status').toLowerCase();
            var trackedDownloadStatus = this.cellValue.has('trackedDownloadStatus') ? this.cellValue.get('trackedDownloadStatus').toLowerCase() : 'ok';
            var icon = 'icon-lidarr-downloading';
            var title = 'Downloading';
            var itemTitle = this.cellValue.get('title');
            var content = itemTitle;

            if (status === 'paused') {
                icon = 'icon-lidarr-paused';
                title = 'Paused';
            }

            if (status === 'queued') {
                icon = 'icon-lidarr-queued';
                title = 'Queued';
            }

            if (status === 'completed') {
                icon = 'icon-lidarr-downloaded';
                title = 'Downloaded';
            }

            if (status === 'pending') {
                icon = 'icon-lidarr-pending';
                title = 'Pending';
            }

            if (status === 'failed') {
                icon = 'icon-lidarr-download-failed';
                title = 'Download failed';
            }

            if (status === 'warning') {
                icon = 'icon-lidarr-download-warning';
                title = 'Download warning: check download client for more details';
            }

            if (trackedDownloadStatus === 'warning') {
                icon += ' icon-lidarr-warning';

                this.templateFunction = Marionette.TemplateCache.get(this.template);
                content = this.templateFunction(this.cellValue.toJSON());
            }

            if (trackedDownloadStatus === 'error') {
                if (status === 'completed') {
                    icon = 'icon-lidarr-import-failed';
                    title = 'Import failed: ' + itemTitle;
                } else {
                    icon = 'icon-lidarr-download-failed';
                    title = 'Download failed';
                }

                this.templateFunction = Marionette.TemplateCache.get(this.template);
                content = this.templateFunction(this.cellValue.toJSON());
            }

            this.$el.html('<i class="{0}"></i>'.format(icon));
            this.$el.popover({
                content   : content,
                html      : true,
                trigger   : 'hover',
                title     : title,
                placement : 'right',
                container : this.$el
            });
        }
        return this;
    }
});