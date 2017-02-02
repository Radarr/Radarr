var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
		template : 'AddMovies/BulkImport/EmptyViewTemplate',


		initialize : function (options) {
			this.templateHelpers = {};
			this.templateHelpers.folder = options.folder;
		}
});
