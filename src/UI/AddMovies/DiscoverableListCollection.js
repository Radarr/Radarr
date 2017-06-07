var Backbone = require('backbone');
var NetImportModel = require('../Settings/NetImport/NetImportModel');
var _ = require('underscore');

var DiscoverableCollection = Backbone.Collection.extend({
		url   : window.NzbDrone.ApiRoot + '/movies/discover/lists',
		model : NetImportModel,
});
var collection = new DiscoverableCollection();
collection.fetch();
module.exports = collection;
