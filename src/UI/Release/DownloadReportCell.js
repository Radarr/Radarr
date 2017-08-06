var Backgrid = require('backgrid');
var AppLayout = require('../AppLayout');
var ForceDownloadView = require('./ForceDownloadView');

module.exports = Backgrid.Cell.extend({
    className : 'download-report-cell',

    events : {
        'click' : '_onClick'
    },

    _onClick : function() {
        if (!this.model.get('downloadAllowed')) {
            var view = new ForceDownloadView({
                model            : this.model,
                targetCollection : this.model.collection
            });
            AppLayout.modalRegion.show(view);

            return;
        }

        var self = this;

        this.$el.html('<i class="icon-sonarr-spinner fa-spin" title="Adding to download queue" />');

        //Using success callback instead of promise so it
        //gets called before the sync event is triggered
        var promise = this.model.save(null, {
            success : function() {
                self.model.set('queued', true);
            }
        });

        promise.fail(function (xhr) {
            if (xhr.responseJSON && xhr.responseJSON.message) {
                self.$el.html('<i class="icon-sonarr-download-failed" title="{0}" />'.format(xhr.responseJSON.message));
            } else {
                self.$el.html('<i class="icon-sonarr-download-failed" title="Failed to add to download queue" />');
            }
        });
    },

    render : function() {
        this.$el.empty();

        if (this.model.get('queued')) {
            this.$el.html('<i class="icon-sonarr-downloading" title="Added to downloaded queue" />');
        } else if (this.model.get('downloadAllowed')) {
            this.$el.html('<i class="icon-sonarr-download" title="Add to download queue" />');
        } else {
            this.$el.html('<i class="icon-radarr-download-warning" title="Force add to download queue."/>');
            this.className = 'force-download-report-cell';
        }

        return this;
    }
});