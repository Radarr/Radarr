var Marionette = require('marionette');
var NetImportCollection = require('./NetImportCollection');
var CollectionView = require('./NetImportCollectionView');
var OptionsView = require('./Options/NetImportOptionsView');
var RootFolderCollection = require('../../AddMovies/RootFolders/RootFolderCollection');
var ImportExclusionsCollection = require('./ImportExclusionsCollection');
var SelectAllCell = require('../../Cells/SelectAllCell');
var DeleteExclusionCell = require('./DeleteExclusionCell');
var ExclusionTitleCell = require("./ExclusionTitleCell");
var _ = require('underscore');
var vent = require('vent');
var Backgrid = require('backgrid');
var $ = require('jquery');

module.exports = Marionette.Layout.extend({
		template : 'Settings/NetImport/NetImportLayoutTemplate',

		regions : {
				lists       : '#x-lists-region',
				listOption : '#x-list-options-region',
				importExclusions : "#exclusions"
		},

		columns: [{
				name: '',
				cell: SelectAllCell,
				headerCell: 'select-all',
				sortable: false
		}, {
				name: 'tmdbId',
				label: 'TMDBID',
				cell: Backgrid.StringCell,
				sortable: false,
		}, {
				name: 'movieTitle',
				label: 'Title',
				cell: ExclusionTitleCell,
				cellValue: 'this',
		}, {
				name: 'this',
				label: '',
				cell: DeleteExclusionCell,
				sortable: false,
		}],


		initialize : function() {
				this.indexersCollection = new NetImportCollection();
				this.indexersCollection.fetch();
				RootFolderCollection.fetch().done(function() {
						RootFolderCollection.synced = true;
				});
				ImportExclusionsCollection.fetch().done(function() {
					ImportExclusionsCollection.synced = true;
				});
		},

		onShow : function() {
				this.listenTo(ImportExclusionsCollection, "sync", this._showExclusions);
				if (ImportExclusionsCollection.synced === true) {
					this._showExclusions();
				}
				this.lists.show(new CollectionView({ collection : this.indexersCollection }));
				this.listOption.show(new OptionsView({ model : this.model }));
		},

		_showExclusions : function() {
			this.exclusionGrid = new Backgrid.Grid({
					collection: ImportExclusionsCollection,
					columns: this.columns,
					className: 'table table-hover'
			});
			this.importExclusions.show(this.exclusionGrid);
		}
});
