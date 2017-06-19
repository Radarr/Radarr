var Marionette = require('marionette');
var ListItemView = require('./ArtistOverviewItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : ListItemView,
    itemViewContainer : '#x-artist-list',
    template          : 'Artist/Index/Overview/ArtistOverviewCollectionViewTemplate'
});