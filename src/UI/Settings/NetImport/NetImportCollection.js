var Backbone = require('backbone');
var NetImportModel = require('./NetImportModel');

module.exports = Backbone.Collection.extend({
		model : NetImportModel,
		url   : window.NzbDrone.ApiRoot + '/netimport',

		comparator : function(left, right, collection) {
				var result = 0;

				return result;
		}
});
