var AppLayout = require("../../AppLayout");
var Marionette = require("marionette");
var ProfileView = require("./ProfileView");
var EditProfileView = require("./Edit/EditProfileLayout");
var ProfileCollection = require("./ProfileSchemaCollection");
var _ = require("underscore");

module.exports = Marionette.CompositeView.extend({
    itemView          : ProfileView,
    itemViewContainer : ".profiles",
    template          : "Settings/Profile/ProfileCollectionTemplate",

    ui : {
        "addCard" : ".x-add-card"
    },

    events : {
        "click .x-add-card" : "_addProfile"
    },

    appendHtml : function(collectionView, itemView, index) {
        collectionView.ui.addCard.parent("li").before(itemView.el);
    },

    _addProfile : function() {
        var self = this;
        var schemaCollection = new ProfileCollection();
        schemaCollection.fetch({
            success : function(collection) {
                var model = _.first(collection.models);
                model.set("id", undefined);
                model.set("name", "");
                model.collection = self.collection;
                var view = new EditProfileView({
                    model             : model,
                    profileCollection : self.collection
                });

                AppLayout.modalRegion.show(view);
            }
        });
    }
});