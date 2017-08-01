var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
//var ButtonsView = require('./ButtonsView');
//var ManualSearchLayout = require('./ManualLayout');
var TitlesCollection = require('./TitlesCollection');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var NoResultsView = require('./NoTitlesView');
var TitleModel = require("./TitleModel");
var TitleCell = require("./TitleCell");
var SourceCell = require("./SourceCell");
var LanguageCell = require("./LanguageCell");

module.exports = Marionette.Layout.extend({
		template : 'Movies/Titles/TitlesLayoutTemplate',

		regions : {
				main : '#movie-titles-region',
				grid : "#movie-titles-grid"
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
						cell  : Backgrid.StringCell
				},
                {
                        name : "this",
                        label : "Source",
                        cell : SourceCell,
                        sortKey : "sourceType",
                },
                {
                        name : "this",
                        label : "Language",
                        cell : LanguageCell
                }
		],


		initialize : function(movie) {
				this.titlesCollection = new TitlesCollection();
				var titles = movie.model.get("alternativeTitles");
				this.movie = movie;
				this.titlesCollection.add(titles);
				//this.listenTo(this.releaseCollection, 'sync', this._showSearchResults);
				this.listenTo(this.model, 'change', function(model, options) {
						if (options && options.changeSource === 'signalr') {
								this._refresh(model);
						}
				});

				//vent.on(vent.Commands.MovieFileEdited, this._showGrid, this);
		},

		_refresh : function(model) {
			this.titlesCollection = new TitlesCollection();
				var file = model.get("alternativeTitles");
				this.titlesCollection.add(file);


			this.onShow();
		},

		_refreshClose : function(options) {
			this.titlesCollection = new TitlesCollection();
			var file = this.movie.model.get("alternativeTitles");
			this.titlesCollection.add(file);
			this._showGrid();
		},

		onShow : function() {
			this.grid.show(new Backgrid.Grid({
					row        : Backgrid.Row,
					columns    : this.columns,
					collection : this.titlesCollection,
					className  : 'table table-hover'
			}));
		},

		_showGrid : function() {
			this.regionManager.get('grid').show(new Backgrid.Grid({
				row        : Backgrid.Row,
				columns    : this.columns,
				collection : this.titlesCollection,
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
