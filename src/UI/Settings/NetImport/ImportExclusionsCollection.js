var Backbone = require('backbone');
var NetImportModel = require('./ImportExclusionModel');

var ImportExclusionsCollection = Backbone.Collection.extend({
		model : NetImportModel,
		url   : window.NzbDrone.ApiRoot + '/exclusions',
});

module.exports = new ImportExclusionsCollection();
