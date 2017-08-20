var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var TrackModel = require('./TrackModel');
require('./TrackCollection');

module.exports = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/track',
    model : TrackModel,

    state : {
        sortKey  : 'trackNumber',
        order    : -1,
        pageSize : 100000
    },

    mode : 'client',

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.artistId = options.artistId;
    },

    byAlbum : function(album) {
        var filtered = this.filter(function(track) {
            return track.get('albumId') === album;
        });

        var TrackCollection = require('./TrackCollection');

        return new TrackCollection(filtered);
    },

    comparator : function(model1, model2) {
        var track1 = model1.get('trackNumber');
        var track2 = model2.get('trackNumber');

        if (track1 < track2) {
            return -1;
        }

        if (track1 > track2) {
            return 1;
        }

        return 0;
    },

    fetch : function(options) {
        if (!this.artistId) {
            throw 'artistId is required';
        }

        if (!options) {
            options = {};
        }

        options.data = { artistId : this.artistId };

        return this.originalFetch.call(this, options);
    }
});