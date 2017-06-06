var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
    template : 'AddMovies/DiscoverableListDropdownViewTemplate',

    initialize : function(lists) {
        this.lists = lists;
    },

    templateHelpers : function() {
        return this.lists;
    }
});
