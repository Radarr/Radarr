var vent = require('vent');
var Marionette = require('marionette');
var moment = require('moment');

module.exports = Marionette.ItemView.extend({
    template : 'Calendar/UpcomingItemViewTemplate',
    tagName  : 'div',

    initialize : function() {
        this.listenTo(this.model, 'change', this.render);
    }
});
