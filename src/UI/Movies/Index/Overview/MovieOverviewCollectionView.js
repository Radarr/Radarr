var Marionette = require('marionette');
var ListItemView = require('./MovieOverviewItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : ListItemView,
    itemViewContainer : '#x-movie-list',
    template          : 'Movies/Index/Overview/MovieOverviewCollectionViewTemplate'
});