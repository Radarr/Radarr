var Marionette = require('marionette');
var ListItemView = require('./SeriesOverviewItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : ListItemView,
    itemViewContainer : '#x-series-list',
    template          : 'Movies/Index/Overview/SeriesOverviewCollectionViewTemplate'
});