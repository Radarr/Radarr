var Backgrid = require('backgrid');
var AppLayout = require('../AppLayout');
var ForceDownloadView = require('./ForceDownloadView');

module.exports = Backgrid.Cell.extend({
    className : 'download-report-cell',

    events : {
        'click' : '_onClick'
    },

    _onClick : function() {
        if (!this.model.downloadOk()) {
            var view = new ForceDownloadView({
                release            : this.model
            });
            AppLayout.modalRegion.show(view);

            return;
        }

        var self = this;

        this.$el.html('<i class="icon-radarr-spinner fa-spin" title="Adding to download queue" />');

        //Using success callback instead of promise so it
        //gets called before the sync event is triggered
        var promise = this.model.save(null, {
            success : function() {
                self.model.set('queued', true);
            }
        });

        promise.fail(function (xhr) {
            if (xhr.responseJSON && xhr.responseJSON.message) {
                self.$el.html('<i class="icon-radarr-download-failed" title="{0}" />'.format(xhr.responseJSON.message));
            } else {
                self.$el.html('<i class="icon-radarr-download-failed" title="Failed to add to download queue" />');
            }
        });
    },

    render : function() {
        this.$el.empty();

        if (this.model.get('queued')) {
            this.$el.html('<i class="icon-radarr-downloading" title="Added to downloaded queue" />');
        } else if (this.model.downloadOk()) {
            this.$el.html('<i class="icon-radarr-download" title="Add to download queue" />');
        } else if (this.model.forceDownloadOk()){
            this.$el.html('<i class="icon-radarr-download-warning" title="Force add to download queue."/>');
            this.className = 'force-download-report-cell';
        } else {
            this.className = 'no-download-report-cell';
        }

        return this;
    }
});