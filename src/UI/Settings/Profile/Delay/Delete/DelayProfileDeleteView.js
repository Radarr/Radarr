var vent = require("vent");
var Marionette = require("marionette");

module.exports = Marionette.ItemView.extend({
    template : "Settings/Profile/Delay/Delete/DelayProfileDeleteViewTemplate",

    events : {
        "click .x-confirm-delete" : "_delete"
    },

    _delete : function() {
        var collection = this.model.collection;

        this.model.destroy({
            wait    : true,
            success : function() {
                vent.trigger(vent.Commands.CloseModalCommand);
            }
        });
    }
});