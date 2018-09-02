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
            var icon = 'icon-radarr-downloading';
            var title = 'Downloading';
            var itemTitle = this.cellValue.get('title');
            var content = itemTitle;

            if (status === 'paused') {
                icon = 'icon-radarr-paused';
                title = 'Paused';
            }

            if (status === 'queued') {
                icon = 'icon-radarr-queued';
                title = 'Queued';
            }

            if (status === 'completed') {
                icon = 'icon-radarr-downloaded';
                title = 'Downloaded';
            }

            if (status === 'pending') {
                icon = 'icon-radarr-pending';
                title = 'Pending';
            }

            if (status === 'failed') {
                icon = 'icon-radarr-download-failed';
                title = 'Download failed';
            }

            if (status === 'warning') {
                icon = 'icon-radarr-download-warning';
                title = 'Download warning: check download client for more details';
            }

            if (trackedDownloadStatus === 'warning') {
                icon += ' icon-radarr-warning';

                this.templateFunction = Marionette.TemplateCache.get(this.template);
                content = this.templateFunction(this.cellValue.toJSON());
            }

            if (trackedDownloadStatus === 'error') {
                if (status === 'completed') {
                    icon = 'icon-radarr-import-failed';
                    title = 'Import failed: ' + itemTitle;
                } else {
                    icon = 'icon-radarr-download-failed';
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