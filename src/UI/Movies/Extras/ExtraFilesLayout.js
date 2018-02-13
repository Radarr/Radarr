var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ExtraFilesCollection = require('./ExtraFilesCollection');
var LoadingView = require('../../Shared/LoadingView');
var ExtraFileModel = require("./ExtraFileModel");
var FileTitleCell = require('../../Cells/FileTitleCell');
var ExtraExtensionCell = require('../../Cells/ExtraExtensionCell');
var ExtraTypeCell = require('../../Cells/ExtraTypeCell');

module.exports = Marionette.Layout.extend({
		template : 'Movies/Extras/ExtraFilesLayoutTemplate',

		regions : {
				main : '#movie-extra-files-region',
				grid : "#movie-extra-files-grid"
		},

		columns : [
			{
				name  : 'relativePath',
				label : 'File',
				cell  : FileTitleCell
			},
			{
				name  : 'extension',
				label : 'Extension',
				cell  : ExtraExtensionCell
			},
			{
				name  : 'type',
				label : 'Type',
				cell  : ExtraTypeCell
			}
		],


		initialize : function() {
			this.collection = new ExtraFilesCollection();
			this.listenTo(this.collection, 'sync', this._showTable);
		},
	
		onRender : function() {
			this.grid.show(new LoadingView());

			this.collection.fetchMovieExtras(this.model.id);
		},
	
		_showTable : function() {
			if (!this.isClosed) {
				this.grid.show(new Backgrid.Grid({
					row        : Backgrid.Row,
					columns    : this.columns,
					collection : this.collection,
					className  : 'table table-hover'
				}));
			}
		}
});
