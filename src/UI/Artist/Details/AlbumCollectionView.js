var _ = require('underscore');
var Marionette = require('marionette');
var SeasonLayout = require('./AlbumLayout');
var AsSortedCollectionView = require('../../Mixins/AsSortedCollectionView');

var view = Marionette.CollectionView.extend({

    itemView : SeasonLayout,

    initialize : function(options) {
        if (!options.trackCollection) {
            throw 'trackCollection is needed';
        }

        this.trackCollection = options.trackCollection;
        this.artist = options.artist;
    },

    itemViewOptions : function() {
        return {
            trackCollection   : this.trackCollection,
            artist            : this.artist
        };
    },

    onTrackGrabbed : function(message) {
        if (message.track.artist.id !== this.trackCollection.artistId) {
            return;
        }

        var self = this;

        _.each(message.track.tracks, function(track) {
            var ep = self.TrackCollection.get(track.id);
            ep.set('downloading', true);
        });

        this.render();
    }
});

AsSortedCollectionView.call(view);

module.exports = view;