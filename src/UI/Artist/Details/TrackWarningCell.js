var NzbDroneCell = require('../../Cells/NzbDroneCell');
var ArtistCollection = require('../ArtistCollection');

module.exports = NzbDroneCell.extend({
    className : 'track-warning-cell',

    render : function() {
        this.$el.empty();

        if (this.model.get('unverifiedSceneNumbering')) {
            this.$el.html('<i class="icon-lidarr-form-warning" title="Scene number hasn\'t been verified yet."></i>');
        }

        else if (ArtistCollection.get(this.model.get('artistId')).get('artistType') === 'anime' && this.model.get('seasonNumber') > 0 && !this.model.has('absoluteEpisodeNumber')) {
            this.$el.html('<i class="icon-lidarr-form-warning" title="Track does not have an absolute track number"></i>');
        }

        this.delegateEvents();
        return this;
    }
});