var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MovieTitleCell = require('../../Cells/MovieListTitleCell');
var BulkImportCollection = require("./BulkImportCollection")
var QualityCell = require('./QualityCell');
var GridPager = require('../../Shared/Grid/Pager');
var SelectAllCell = require('../../Cells/SelectAllCell');
var MoviePathCell = require("./MoviePathCell")
var LoadingView = require('../../Shared/LoadingView');
var EmptyView = require("./EmptyView");

module.exports = Marionette.Layout.extend({
		template : 'AddMovies/BulkImport/BulkImportViewTemplate',

		regions : {
				table : '#x-movies-bulk',
				pager : '#x-movies-bulk-pager'
		},

		columns : [
				{
						name     : 'this',
						label    : 'Movie',
						cell     : MovieTitleCell
				},
				{
					name : "this",
					label : "Path",
					cell : MoviePathCell,
				},
				{
						name     : 'this',
						label    : 'Quality',
						cell     : QualityCell,
						sortable : false
				},
				{
					name : "",
					label : "",
					cell : SelectAllCell,
					headerCell : 'select-all',
					sortable : false
				}
		],

		initialize : function(options) {
				this.bulkImportCollection = new BulkImportCollection();
				this.model = options.model;
				this.folder = this.model.get("path");
				this.bulkImportCollection.fetch({ data : { folder : this.folder, id : this.model.get("id") }})
				this.listenTo(this.bulkImportCollection, 'all', this._showTable);
		},

		onShow : function() {
			this.table.show(new LoadingView());
				//this._showTable();
		},

		_showTable : function() {
				if (this.bulkImportCollection.length === 0) {
					this.table.show(new EmptyView({ folder : this.folder }));
					return;
				}

				this.table.show(new Backgrid.Grid({
						columns    : this.columns,
						collection : this.bulkImportCollection,
						className  : 'table table-hover'
				}));

				this.pager.show(new GridPager({
						columns    : this.columns,
						collection : this.bulkImportCollection
				}));
		}
});
