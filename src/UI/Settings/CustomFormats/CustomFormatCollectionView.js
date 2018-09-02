var Marionette = require('marionette');
var ItemView = require('./CustomFormatItemView');
var SchemaModal = require('./Add/CustomFormatSchemaModal');

module.exports = Marionette.CompositeView.extend({
    itemView          : ItemView,
    itemViewContainer : '.indexer-list',
    template          : 'Settings/CustomFormats/CustomFormatCollectionViewTemplate',

    ui : {
        'addCard' : '.x-add-card'
    },

    events : {
        'click .x-add-card' : '_openSchemaModal'
    },

    appendHtml : function(collectionView, itemView, index) {
        collectionView.ui.addCard.parent('li').before(itemView.el);
    },

    _openSchemaModal : function() {
        SchemaModal.open(this.collection);
    }
});
