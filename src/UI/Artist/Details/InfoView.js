var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Artist/Details/InfoViewTemplate',

    initialize : function(options) {
        this.trackFileCollection = options.trackFileCollection;

        this.listenTo(this.model, 'change', this.render);
        this.listenTo(this.trackFileCollection, 'sync', this.render);
    },

    templateHelpers : function() {
        return {
            fileCount : this.trackFileCollection.length
        };
    }
});