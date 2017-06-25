var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Artist/Details/AlbumInfoViewTemplate',

    initialize : function(options) {

        this.listenTo(this.model, 'change', this.render);
    },

});