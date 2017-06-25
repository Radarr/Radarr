var Marionette = require('marionette');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'track-progress-cell',
    template  : 'Cells/TrackProgressCellTemplate',

    render : function() {

        var trackCount = this.model.get('trackCount');
        var trackFileCount = this.model.get('trackFileCount');

        var percent = 100;

        if (trackCount > 0) {
            percent = trackFileCount / trackCount * 100;
        }

        this.model.set('percentOfTracks', percent);

        this.templateFunction = Marionette.TemplateCache.get(this.template);
        var data = this.model.toJSON();
        var html = this.templateFunction(data);
        this.$el.html(html);

        return this;
    }
});