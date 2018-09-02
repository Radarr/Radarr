var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./Edit/CustomFormatEditView');
require('./FormatTagHelpers');

module.exports = Marionette.ItemView.extend({
    template : 'Settings/CustomFormats/CustomFormatItemViewTemplate',
    tagName  : 'li',

    events : {
        'click' : '_edit'
    },

    initialize : function() {
        this.listenTo(this.model, 'sync', this.render);
    },

    _edit : function() {
        var view = new EditView({
            model            : this.model,
            targetCollection : this.model.collection
        });
        AppLayout.modalRegion.show(view);
    }
});
