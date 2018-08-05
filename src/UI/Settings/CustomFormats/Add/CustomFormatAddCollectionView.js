var ThingyAddCollectionView = require('../../ThingyAddCollectionView');
var ThingyHeaderGroupView = require('../../ThingyHeaderGroupView');
var AddItemView = require('./CustomFormatAddItemView');

module.exports = ThingyAddCollectionView.extend({
    itemView          : ThingyHeaderGroupView.extend({ itemView : AddItemView }),
    itemViewContainer : '.add-indexer .items',
    template          : 'Settings/CustomFormats/Add/CustomFormatAddCollectionViewTemplate'
});
