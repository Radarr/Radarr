var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'title-source-cell',

    render : function() {
        this.$el.empty();

        var link;
        var sourceTitle = this.model.get("sourceType");
        var sourceId = this.model.get("sourceId");

        switch (sourceTitle) {
            case "tmdb":
                sourceTitle = "TMDB";
                link = "https://themoviedb.org/movie/" + sourceId;
                break;
            case "mappings":
                sourceTitle = "Radarr Mappings";
                link = "https://mappings.radarr.video/mapping/" + sourceId;
                break;
            case "user":
                sourceTitle = "Force Download";
                break;
            case "indexer":
                sourceTitle = "Indexer";
                break;
        }

        var a = "{0}";

        if (link) {
            a = "<a href='"+link+"' target='_blank'>{0}</a>";
        }

        this.$el.html(a.format(sourceTitle));

        return this;
    }


});
