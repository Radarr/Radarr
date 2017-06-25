var reqres = require('../reqres');
var Backbone = require('backbone');
var NzbDroneCell = require('./NzbDroneCell');
var QueueCollection = require('../Activity/Queue/QueueCollection');
var moment = require('moment');
var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'track-status-cell',

    render : function() {
        this.listenTo(QueueCollection, 'sync', this._renderCell);

        this._renderCell();

        return this;
    },

    _renderCell : function() {

        if (this.trackFile) {
            this.stopListening(this.trackFile, 'change', this._refresh);
        }

        this.$el.empty();

        if (this.model) {

            var icon;
            var tooltip;

            var hasAired = moment(this.model.get('airDateUtc')).isBefore(moment());
            this.trackFile = this._getFile();

            if (this.trackFile) {
                this.listenTo(this.trackFile, 'change', this._refresh);

                var quality = this.trackFile.get('quality');
                var revision = quality.revision;
                var size = FormatHelpers.bytes(this.trackFile.get('size'));
                var title = 'Track downloaded';

                if (revision.real && revision.real > 0) {
                    title += '[REAL]';
                }

                if (revision.version && revision.version > 1) {
                    title += ' [PROPER]';
                }

                if (size !== '') {
                    title += ' - {0}'.format(size);
                }

                if (this.trackFile.get('qualityCutoffNotMet')) {
                    this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
                } else {
                    this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
                }

                return;
            }

            else {
                var model = this.model;
                var downloading = false; //TODO Fix this by adding to QueueCollection

                if (downloading) {
                    var progress = 100 - (downloading.get('sizeleft') / downloading.get('size') * 100);

                    if (progress === 0) {
                        icon = 'icon-lidarr-downloading';
                        tooltip = 'Track is downloading';
                    }

                    else {
                        this.$el.html('<div class="progress" title="Track is downloading - {0}% {1}">'.format(progress.toFixed(1), downloading.get('title')) +
                                      '<div class="progress-bar progress-bar-purple" style="width: {0}%;"></div></div>'.format(progress));
                        return;
                    }
                }

                else if (this.model.get('grabbed')) {
                    icon = 'icon-lidarr-downloading';
                    tooltip = 'Track is downloading';
                }

                else if (!this.model.get('airDateUtc')) {
                    icon = 'icon-lidarr-tba';
                    tooltip = 'TBA';
                }

                else if (hasAired) {
                    icon = 'icon-lidarr-missing';
                    tooltip = 'Track missing from disk';
                } else {
                    icon = 'icon-lidarr-not-aired';
                    tooltip = 'Track has not aired';
                }
            }

            this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, tooltip));
        }
    },

    _getFile : function() {
        var hasFile = this.model.get('hasFile');

        if (hasFile) {
            var trackFile;

            if (reqres.hasHandler(reqres.Requests.GetTrackFileById)) {
                trackFile = reqres.request(reqres.Requests.GetTrackFileById, this.model.get('trackFileId'));
            }

            else if (this.model.has('trackFile')) {
                trackFile = new Backbone.Model(this.model.get('trackFile'));
            }

            if (trackFile) {
                return trackFile;
            }
        }

        return undefined;
    }
});