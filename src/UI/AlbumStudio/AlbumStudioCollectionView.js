var _ = require('underscore');
var Marionette = require('marionette');
var SingleAlbumCell = require('./SingleAlbumCell');
var AsSortedCollectionView = require('../Mixins/AsSortedCollectionView');

var view = Marionette.CollectionView.extend({

    itemView : SingleAlbumCell,

    initialize : function(options) {
        this.albumCollection = options.collection;
        this.artist = options.artist;
    },

    itemViewOptions : function() {
        return {
            albumCollection   : this.albumCollection,
            artist            : this.artist
        };
    }
});

AsSortedCollectionView.call(view);

module.exports = view;