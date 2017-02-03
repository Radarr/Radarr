var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
var MovieModel = require('../../Movies/MovieModel');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPageableCollection = require('../../Mixins/AsPageableCollection');

var BulkImportCollection = PageableCollection.extend({
		url   : window.NzbDrone.ApiRoot + '/movies/bulkimport',
		model : MovieModel,
		tableName : 'bulkimport',

		state : {
			pageSize : 15,
			sortKey: 'sortTitle'
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
		}
});


BulkImportCollection = AsSortedCollection.call(BulkImportCollection);
BulkImportCollection = AsPageableCollection.call(BulkImportCollection);

module.exports = BulkImportCollection;
