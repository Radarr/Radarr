var Marionette = require('marionette');
var FormatHelpers = require('../../Shared/FormatHelpers');

module.exports = Marionette.ItemView.extend({
    template : 'Artist/Details/AlbumInfoViewTemplate',

    initialize : function(options) {

        this.listenTo(this.model, 'change', this.render);
    },

    templateHelpers : function() {
        return {
            durationMin : FormatHelpers.timeMinSec(this.model.get('duration'))
        };
    }

});