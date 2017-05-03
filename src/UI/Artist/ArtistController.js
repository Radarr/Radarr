var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var ArtistCollection = require('./ArtistCollection');
var SeriesIndexLayout = require('./Index/SeriesIndexLayout');
var SeriesDetailsLayout = require('../Series/Details/SeriesDetailsLayout');

module.exports = NzbDroneController.extend({
    _originalInit : NzbDroneController.prototype.initialize,

    initialize : function() {
        this.route('', this.series);
        this.route('artist', this.series);
        this.route('artist/:query', this.seriesDetails);

        this._originalInit.apply(this, arguments);
    },

    artist : function() {
        this.setTitle('Lidarr');
        this.setArtistName('Lidarr');
        this.showMainRegion(new SeriesIndexLayout());
    },

    seriesDetails : function(query) {
        var artists = ArtistCollection.where({ artistNameSlug : query });
        console.log('seriesDetails, artists: ', artists);
        if (artists.length !== 0) {
            var targetSeries = artists[0];
            console.log("[ArtistController] targetSeries: ", targetSeries);
            this.setTitle(targetSeries.get('title'));
            this.setArtistName(targetSeries.get('artistName'));
            this.showMainRegion(new SeriesDetailsLayout({ model : targetSeries }));
        } else {
            this.showNotFound();
        }
    }
});