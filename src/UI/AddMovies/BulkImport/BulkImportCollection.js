var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
var MovieModel = require('../../Movies/MovieModel');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPageableCollection = require('../../Mixins/AsPageableCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var BulkImportCollection = PageableCollection.extend({
		url   : window.NzbDrone.ApiRoot + '/movies/bulkimport',
		model : MovieModel,
		mode: "infinite",
		tableName : 'bulkimport',

		state : {
			pageSize : 15,
			sortKey: 'sortTitle',
			firstPage: 1
		},

		queryParams: {
			totalPages: null,
			totalRecords: null,
			sortKey: "sort",
			order: "direction",
			directions: {
			"-1": "asc",
			"1": "desc"
		}
  },

		// queryParams : {
		// 	totalPages : null,
		// 	totalRecords : null,
		// 	pageSize : 'pageSize',
		// 	sortKey : 'sortKey'
		// },

		/*parse : function(response) {
				var self = this;

				_.each(response.records, function(model) {
						model.id = undefined;
				});

				return response;
		},*/

		parseState : function(resp) {
				return { totalRecords : resp.totalRecords };
		},

		parseRecords : function(resp) {
				if (resp) {
						return resp.records;
				}

				return resp;
		},

		fetch : function(options) {

			options = options || {};

			var data = options.data || {};

			if (data.id === undefined || data.folder === undefined) {
				data.id = this.folderId;
				data.folder = this.folder;
			}

			options.data = data;

			return PageableCollection.prototype.fetch.call(this, options);
		},

		parseLinks : function(options) {
			console.log(options);
			return {
				first : this.url,
				next: this.url,
				last : this.url
			};
		}
});


BulkImportCollection = AsSortedCollection.call(BulkImportCollection);
BulkImportCollection = AsPageableCollection.call(BulkImportCollection);
BulkImportCollection = AsPersistedStateCollection.call(BulkImportCollection);

module.exports = BulkImportCollection;
