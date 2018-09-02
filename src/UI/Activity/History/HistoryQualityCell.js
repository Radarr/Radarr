var NzbDroneCell = require('../../Cells/NzbDroneCell');
var _ = require('underscore');

module.exports = NzbDroneCell.extend({
    className : 'history-quality-cell',

    render : function() {

        var title = '';
        var quality = this.model.get('quality');
        var revision = quality.revision;

        if (revision.real && revision.real > 0) {
            title += ' REAL';
        }

        if (revision.version && revision.version > 1) {
            title += ' PROPER';
        }

        title = title.trim();

        var html = '';

        if (this.model.get('qualityCutoffNotMet')) {
            html = '<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name);
        } else {
            html = '<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name);
        }

        if (quality.customFormats.length > 0){
            var formatNames = _.map(quality.customFormats, function(format) {
                return format.name;
            });
            html += ' <span class="badge badge-success" title="Custom Formats">{0}</span>'.format(formatNames.join(", "));
        }

        this.$el.html(html);

        return this;
    }
});
