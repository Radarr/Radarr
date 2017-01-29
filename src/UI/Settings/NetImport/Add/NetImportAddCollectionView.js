var ThingyAddCollectionView = require('../../ThingyAddCollectionView');
var ThingyHeaderGroupView = require('../../ThingyHeaderGroupView');
var AddItemView = require('./NetImportAddItemView');

module.exports = ThingyAddCollectionView.extend({
		itemView          : ThingyHeaderGroupView.extend({ itemView : AddItemView }),
		itemViewContainer : '.add-indexer .items',
		template          : 'Settings/NetImport/Add/NetImportAddCollectionViewTemplate'
});
