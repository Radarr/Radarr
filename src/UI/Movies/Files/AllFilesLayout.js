var vent = require('vent');
var Marionette = require('marionette');
var FilesLayout = require('./Media/FilesLayout');
var ExtraFilesLayout = require('./Extras/ExtraFilesLayout');

module.exports = Marionette.Layout.extend({
		template : 'Movies/Files/AllFilesLayoutTemplate',

		regions : {
			files        : "#movie-files",
			mediaFiles   : "#movie-media-files",
			extras  	 : "#movie-extra-files"
		},

		onShow : function() {
			this.filesLayout = new FilesLayout({ model : this.model });
			this.extraFilesLayout = new ExtraFilesLayout({ model : this.model });

			this._showFiles();
		},

		_showFiles : function(e) {
			if (e) {
				e.preventDefault();
			}
			
			this.mediaFiles.show(this.filesLayout);
			this.extras.show(this.extraFilesLayout);
		}
});
