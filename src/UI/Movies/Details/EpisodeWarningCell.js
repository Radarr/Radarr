var NzbDroneCell = require("../../Cells/NzbDroneCell");
var SeriesCollection = require("../SeriesCollection");

module.exports = NzbDroneCell.extend({
    className : "episode-warning-cell",

    render : function() {
        this.$el.empty();

        if (this.model.get("unverifiedSceneNumbering")) {
            this.$el.html('<i class="icon-sonarr-form-warning" title="Scene number hasn\"t been verified yet."></i>');
        }

        else if (SeriesCollection.get(this.model.get("seriesId")).get("seriesType") === "anime" && this.model.get("seasonNumber") > 0 && !this.model.has("absoluteEpisodeNumber")) {
            this.$el.html('<i class="icon-sonarr-form-warning" title="Episode does not have an absolute episode number"></i>');
        }

        this.delegateEvents();
        return this;
    }
});
