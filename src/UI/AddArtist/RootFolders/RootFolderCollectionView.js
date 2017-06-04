var Marionette = require('marionette');
var RootFolderItemView = require('./RootFolderItemView');

module.exports = Marionette.CompositeView.extend({
    template          : 'AddArtist/RootFolders/RootFolderCollectionViewTemplate',
    itemViewContainer : '.x-root-folders',
    itemView          : RootFolderItemView
});