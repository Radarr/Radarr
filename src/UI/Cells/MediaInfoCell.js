var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'release-title-cell',

    render : function() {
        this.$el.empty();

        var info = this.model.get('mediaInfo');
        if (info) {
          var runtime = info.runTime;
          if (runtime) {
            runtime = runtime.split(".")[0];
          }
          var video = "{0} ({1}x{2}) ({3})".format(info.videoFormat || info.videoCodec, info.width, info.height, runtime);
          var audio = "{0} ({1})".format(info.audioFormat, info.audioLanguages);
          this.$el.html(video + " " + audio);
        }


        return this;
    }
});
