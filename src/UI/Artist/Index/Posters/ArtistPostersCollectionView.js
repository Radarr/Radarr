var Marionette = require('marionette');
var PosterItemView = require('./ArtistPostersItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : PosterItemView,
    itemViewContainer : '#x-artist-posters',
    template          : 'Artist/Index/Posters/ArtistPostersCollectionViewTemplate'
});