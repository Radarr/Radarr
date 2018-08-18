var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Settings/CustomFormats/DeleteCustomFormatView',

    ui: {
        indicator : '.x-indicator',
        delete : '.x-confirm-delete',
        cancel : '.x-cancel-confirm'
    },

    events : {
        'click .x-confirm-delete' : '_removeProfile'
    },

    _removeProfile : function() {
        this.ui.indicator.show();
        this.ui.delete.attr("disabled", "disabled");
        this.ui.cancel.attr("disabled", "disabled");

        var self = this;
        this.model.destroy({ wait : true }).done(function() {
            self.ui.indicator.hide();
            vent.trigger(vent.Commands.CloseModalCommand);
        });
    }
});
