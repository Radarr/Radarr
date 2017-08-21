var AppLayout = require("../../AppLayout");
var Marionette = require("marionette");
var EditView = require("./Edit/NetImportEditView");

module.exports = Marionette.ItemView.extend({
        template : "Settings/NetImport/NetImportItemViewTemplate",
        tagName  : "li",

        events : {
                "click" : "_edit"
        },

        initialize : function() {
                this.listenTo(this.model, "sync", this.render);
        },

        _edit : function() {
                var view = new EditView({
                        model            : this.model,
                        targetCollection : this.model.collection
                });
                AppLayout.modalRegion.show(view);
        }
});
