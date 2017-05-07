var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var ArtistCollection = require('./ArtistCollection');
var SeriesIndexLayout = require('../Series/Index/SeriesIndexLayout');
var SeriesDetailsLayout = require('../Series/Details/SeriesDetailsLayout');

module.exports = NzbDroneController.extend({
    _originalInit : NzbDroneController.prototype.initialize,

    initialize : function() {
        this.route('', this.artist);
        this.route('artist', this.artist);
        this.route('artist/:query', this.artistDetails);

        this._originalInit.apply(this, arguments);
    },

    artist : function() {
        this.setTitle('Lidarr');
        this.showMainRegion(new SeriesIndexLayout());
    },

    artistDetails : function(query) {
        var artists = ArtistCollection.where({ artistNameSlug : query });
        console.log('artistDetails, artists: ', artists);
        if (artists.length !== 0) {
            var targetSeries = artists[0];
            console.log("[ArtistController] targetSeries: ", targetSeries);
            this.setTitle(targetSeries.get('artistName')); // TODO: Update NzbDroneController
            //this.setArtistName(targetSeries.get('artistName'));
            this.showMainRegion(new SeriesDetailsLayout({ model : targetSeries }));
        } else {
            this.showNotFound();
        }
    }
});