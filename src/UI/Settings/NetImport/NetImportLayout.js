var Marionette = require('marionette');
var NetImportCollection = require('./NetImportCollection');
var CollectionView = require('./NetImportCollectionView');
var OptionsView = require('./Options/NetImportOptionsView');

module.exports = Marionette.Layout.extend({
		template : 'Settings/NetImport/NetImportLayoutTemplate',

		regions : {
				lists       : '#x-lists-region',
				listOption : '#x-list-options-region',
		},

		initialize : function() {
				this.indexersCollection = new NetImportCollection();
				this.indexersCollection.fetch();
		},

		onShow : function() {
				this.lists.show(new CollectionView({ collection : this.indexersCollection }));
				this.listOption.show(new OptionsView({ model : this.model }));
		}
});
