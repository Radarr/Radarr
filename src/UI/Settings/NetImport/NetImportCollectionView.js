var Marionette = require('marionette');
var ItemView = require('./NetImportItemView');
var SchemaModal = require('./Add/NetImportSchemaModal');

module.exports = Marionette.CompositeView.extend({
		itemView          : ItemView,
		itemViewContainer : '.list-list',
		template          : 'Settings/NetImport/NetImportCollectionViewTemplate',

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
