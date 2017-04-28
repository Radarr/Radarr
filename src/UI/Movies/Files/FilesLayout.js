var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
//var ButtonsView = require('./ButtonsView');
//var ManualSearchLayout = require('./ManualLayout');
var FilesCollection = require('./FilesCollection');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var NoResultsView = require('./NoFilesView');
var FileModel = require("./FileModel");
var FileTitleCell = require('../../Cells/FileTitleCell');
var FileSizeCell = require('../../Cells/FileSizeCell');
var QualityCell = require('../../Cells/QualityCell');
var MediaInfoCell = require('../../Cells/MediaInfoCell');
var ApprovalStatusCell = require('../../Cells/ApprovalStatusCell');
var DownloadReportCell = require('../../Release/DownloadReportCell');
var AgeCell = require('../../Release/AgeCell');
var ProtocolCell = require('../../Release/ProtocolCell');
var PeersCell = require('../../Release/PeersCell');
var EditionCell = require('../../Cells/EditionCell');
var DeleteFileCell = require("./DeleteFileCell");
var EditFileCell = require("./EditFileCell");

module.exports = Marionette.Layout.extend({
		template : 'Movies/Files/FilesLayoutTemplate',

		regions : {
				main : '#movie-files-region',
				grid : "#movie-files-grid"
		},

		events : {
				'click .x-search-auto'   : '_searchAuto',
				'click .x-search-manual' : '_searchManual',
				'click .x-search-back'   : '_showButtons'
		},

		columns : [
				{
						name  : 'title',
						label : 'Title',
						cell  : FileTitleCell
				},
				{
						name : "mediaInfo",
						label : "Media Info",
						cell : MediaInfoCell
				},
				{
						name  : 'edition',
						label : 'Edition',
						cell  : EditionCell,
						title : "Edition",
				},
				{
						name  : 'size',
						label : 'Size',
						cell  : FileSizeCell
				},
				{
						name  : 'quality',
						label : 'Quality',
						cell  : QualityCell,
				},
				{
					name : "delete",
					label : "",
					cell : DeleteFileCell,
				},
				{
					name : "edit",
					label : "",
					cell : EditFileCell,
				}
		],


		initialize : function(movie) {
				this.filesCollection = new FilesCollection();
				var file = movie.model.get("movieFile");
				this.movie = movie;
				this.filesCollection.add(file);
				//this.listenTo(this.releaseCollection, 'sync', this._showSearchResults);
				this.listenTo(this.model, 'change', function(model, options) {
						if (options && options.changeSource === 'signalr') {
								this._refresh(model);
						}
				});

				vent.on(vent.Commands.MovieFileEdited, this._showGrid, this);
		},

		_refresh : function(model) {
			this.filesCollection = new FilesCollection();

			if(model.get('hasFile')) {
				var file = model.get("movieFile");
				this.filesCollection.add(file);
			}

			this.onShow();
		},

		_refreshClose : function(options) {
			this.filesCollection = new FilesCollection();
			var file = this.movie.model.get("movieFile");
			this.filesCollection.add(file);
			this._showGrid();
		},

		onShow : function() {
			this.grid.show(new Backgrid.Grid({
					row        : Backgrid.Row,
					columns    : this.columns,
					collection : this.filesCollection,
					className  : 'table table-hover'
			}));
		},

		_showGrid : function() {
			this.regionManager.get('grid').show(new Backgrid.Grid({
				row        : Backgrid.Row,
				columns    : this.columns,
				collection : this.filesCollection,
				className  : 'table table-hover'
			}));
		},

		_showMainView : function() {
				this.main.show(this.mainView);
		},

		_showButtons : function() {
				this._showMainView();
		},

		_showSearchResults : function() {
				if (this.releaseCollection.length === 0) {
						this.mainView = new NoResultsView();
				}

				else {
						//this.mainView = new ManualSearchLayout({ collection : this.releaseCollection });
				}

				this._showMainView();
		}
});
