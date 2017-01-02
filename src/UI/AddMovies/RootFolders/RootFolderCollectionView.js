var Marionette = require('marionette');
var RootFolderItemView = require('./RootFolderItemView');

module.exports = Marionette.CompositeView.extend({
    template          : 'AddMovies/RootFolders/RootFolderCollectionViewTemplate',
    itemViewContainer : '.x-root-folders',
    itemView          : RootFolderItemView
});