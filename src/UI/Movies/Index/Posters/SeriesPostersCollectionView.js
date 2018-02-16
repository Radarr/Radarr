var Marionette = require('marionette');
var PosterItemView = require('./SeriesPostersItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : PosterItemView,
    itemViewContainer : '#x-series-posters',
    template          : 'Movies/Index/Posters/SeriesPostersCollectionViewTemplate'
});