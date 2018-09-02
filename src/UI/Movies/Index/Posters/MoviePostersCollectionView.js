var Marionette = require('marionette');
var PosterItemView = require('./MoviePostersItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : PosterItemView,
    itemViewContainer : '#x-movie-posters',
    template          : 'Movies/Index/Posters/MoviePostersCollectionViewTemplate'
});