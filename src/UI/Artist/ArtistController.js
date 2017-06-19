var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var ArtistCollection = require('./ArtistCollection');
var ArtistIndexLayout = require('./Index/ArtistIndexLayout');
var ArtistDetailsLayout = require('./Details/ArtistDetailsLayout');

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
        this.showMainRegion(new ArtistIndexLayout());
    },

    artistDetails : function(query) {
        var artists = ArtistCollection.where({ nameSlug : query });
        console.log('artistDetails, artists: ', artists);
        if (artists.length !== 0) {
            var targetArtist = artists[0];
            console.log("[ArtistController] targetArtist: ", targetArtist);
            this.setTitle(targetArtist.get('name')); // TODO: Update NzbDroneController
            //this.setArtistName(targetSeries.get('artistName'));
            this.showMainRegion(new ArtistDetailsLayout({ model : targetArtist }));
        } else {
            this.showNotFound();
        }
    }
});