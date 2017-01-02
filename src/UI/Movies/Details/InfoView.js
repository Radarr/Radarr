var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Details/InfoViewTemplate',

    initialize : function(options) {
        //this.episodeFileCollection = options.episodeFileCollection;

        this.listenTo(this.model, 'change', this.render);
        //this.listenTo(this.episodeFileCollection, 'sync', this.render); TODO: Update this;
    },

    templateHelpers : function() {
        return {
            fileCount : 0
        };
    }
});
