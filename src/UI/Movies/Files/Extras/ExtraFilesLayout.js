var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ExtraFilesCollection = require('./ExtraFilesCollection');
var LoadingView = require('../../../Shared/LoadingView');
var ExtraFileModel = require("./ExtraFileModel");
var FileTitleCell = require('../../../Cells/FileTitleCell');
var ExtraExtensionCell = require('../../../Cells/ExtraExtensionCell');
var ExtraTypeCell = require('../../../Cells/ExtraTypeCell');
var NoResultsView = require('../NoFilesView');

module.exports = Marionette.Layout.extend({
		template : 'Movies/Files/Extras/ExtraFilesLayoutTemplate',

		regions : {
			extraFilesTable : '.extra-files-table'
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
	
		onShow : function() {
			this.extraFilesTable.show(new LoadingView());
			
			this.collection.fetchMovieExtras(this.model.id);			
		},
	
		_showTable : function() {
			if (this.collection.any()) {
				this.extraFilesTable.show(new Backgrid.Grid({
					row        : Backgrid.Row,
					columns    : this.columns,
					collection : this.collection,
					className  : 'table table-hover'
				}));
			} else {
				this.extraFilesTable.show(new NoResultsView());
			}
		}
});
