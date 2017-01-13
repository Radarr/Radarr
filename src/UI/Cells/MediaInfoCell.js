var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'release-title-cell',

    render : function() {
        this.$el.empty();

        var info = this.model.get('mediaInfo');
        var video = info.videoCodec;
        var audio = info.audioFormat;
        this.$el.html(video + " " + audio);

        return this;
    }
});
