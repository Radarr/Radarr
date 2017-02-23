var $ = require('jquery');
var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MovieTitleCell = require('./BulkImportMovieTitleCell');
var BulkImportCollection = require("./BulkImportCollection");
var QualityCell = require('./QualityCell');
var TmdbIdCell = require('./TmdbIdCell');
var GridPager = require('../../Shared/Grid/Pager');
var SelectAllCell = require('./BulkImportSelectAllCell');
var ProfileCell = require('./BulkImportProfileCellT');
var MonitorCell = require('./BulkImportMonitorCell');
var MoviePathCell = require("./MoviePathCell");
var LoadingView = require('../../Shared/LoadingView');
var EmptyView = require("./EmptyView");
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var CommandController = require('../../Commands/CommandController');
var Messenger = require('../../Shared/Messenger');
var MoviesCollection = require('../../Movies/MoviesCollection');
var ProfileCollection = require('../../Profile/ProfileCollection');

require('backgrid.selectall');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
		template : 'AddMovies/BulkImport/BulkImportViewTemplate',

		regions : {
				toolbar : '#x-toolbar',
				table : '#x-movies-bulk',
				pager : '#x-movies-bulk-pager'
		},

		ui : {
			addSelectdBtn : '.x-add-selected',
			//addAllBtn : '.x-add-all',
			pageSizeSelector : '.x-page-size'
		},

		events: { "change .x-page-size" : "_pageSizeChanged" },

		initialize : function(options) {
				ProfileCollection.fetch();
				this.bulkImportCollection = new BulkImportCollection().bindSignalR({ updateOnly : true });
				this.model = options.model;
				this.folder = this.model.get("path");
				this.folderId = this.model.get("id");
				this.bulkImportCollection.folderId = this.folderId;
				this.bulkImportCollection.folder = this.folder;
				this.bulkImportCollection.fetch();
				this.listenTo(this.bulkImportCollection, {"sync" : this._showContent, "error" : this._showContent, "backgrid:selected" : this._select});
		},

		_pageSizeChanged : function(event) {
			var pageSize = parseInt($(event.target).val());
			this.bulkImportCollection.fullCollection.reset();
			this.bulkImportCollection.reset();
            this.table.show(new LoadingView());
			//debugger;
			this.bulkImportCollection.setPageSize(pageSize);
			//this.bulkImportCollection.fetch();
		},

		columns : [
				{
					name : '',
					cell : SelectAllCell,
					headerCell : 'select-all',
					sortable : false,
					cellValue : 'this'
				},
				{
					name     : 'movie',
					label    : 'Movie',
					cell     : MovieTitleCell,
					cellValue : 'this',
					sortable : false,
				},
				{
					name : "path",
					label : "Path",
					cell : MoviePathCell,
					cellValue : 'this',
					sortable : false,
				},
				{
					name	: 'tmdbId',
					label	: 'Tmdb Id',
					cell	: TmdbIdCell,
					cellValue : 'this',
					sortable: false
				},
				{
					name :'monitor',
					label: 'Monitor',
					cell : MonitorCell,
					cellValue : 'this'
				},
				{
					name : 'profileId',
					label : 'Profile',
					cell  : ProfileCell,
					cellValue : "this",
				},
				{
					name     : 'quality',
					label    : 'Quality',
					cell     : QualityCell,
					cellValue : 'this',
					sortable : false
				}
		],

		_showContent : function() {
			this._showToolbar();
			this._showTable();
		},

		onShow : function() {
			this.table.show(new LoadingView());
		},

		_showToolbar : function() {
			var leftSideButtons = {
				type : 'default',
				storeState: false,
				collapse : true,
				items : [
					{
						title        : 'Add Selected',
						icon         : 'icon-sonarr-add',
						callback     : this._addSelected,
						ownerContext : this,
						className    : 'x-add-selected'
					}//,
					// {
					// 	title        : 'Add All',
					// 	icon         : 'icon-sonarr-add',
					// 	callback     : this._addAll,
					// 	ownerContext : this,
					// 	className    : 'x-add-all'
					// }
				]
			};

			this.toolbar.show(new ToolbarLayout({
				left    : [leftSideButtons],
				right   : [],
				context : this
			}));

			$('#x-toolbar').addClass('inline');
		},

		_addSelected : function() {
			var selected = _.filter(this.bulkImportCollection.fullCollection.models, function(elem){
				return elem.selected;
			});
			console.log(selected);

			var promise = MoviesCollection.importFromList(selected);
			this.ui.addSelectdBtn.spinForPromise(promise);
			this.ui.addSelectdBtn.addClass('disabled');
			//this.ui.addAllBtn.addClass('disabled');

			if (selected.length === 0) {
				Messenger.show({
					type    : 'error',
					message : 'No movies selected'
				});
				return;
			}

			Messenger.show({
				message : "Importing {0} movies. This can take multiple minutes depending on how many movies should be imported. Don't close this browser window until it is finished!".format(selected.length),
				hideOnNavigate : false,
				hideAfter : 30,
				type : "error"
			});

			var _this = this;

			promise.done(function() {
				Messenger.show({
					message        : "Imported movies from folder.",
					hideAfter      : 8,
					hideOnNavigate : true
				});


				_.forEach(selected, function(movie) {
					movie.destroy(); //update the collection without the added movies
				});
			});
		},

		_addAll : function() {
			console.log("TODO");
		},

		_handleEvent : function(event_name, data) {
			if (event_name === "sync" || event_name === "content") {
				this._showContent();
			}
		},

		_select : function(model, selected) {
			model.selected = selected;
		},

		_showTable : function() {
				if (this.bulkImportCollection.length === 0) {
					this.table.show(new EmptyView({ folder : this.folder }));
					return;
				}

				//TODO: override row in order to set an opacity based on duplication state of the movie
				this.importGrid = new Backgrid.Grid({
						columns    : this.columns,
						collection : this.bulkImportCollection,
						className  : 'table table-hover'
				});

				this.table.show(this.importGrid);

				this.pager.show(new GridPager({
						columns    : this.columns,
						collection : this.bulkImportCollection
				}));
		}
});
