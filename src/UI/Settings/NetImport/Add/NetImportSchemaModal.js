var _ = require('underscore');
var AppLayout = require('../../../AppLayout');
var Backbone = require('backbone');
var SchemaCollection = require('../NetImportCollection');
var AddCollectionView = require('./NetImportAddCollectionView');

module.exports = {
		open : function(collection) {
				var schemaCollection = new SchemaCollection();
				var originalUrl = schemaCollection.url;
				schemaCollection.url = schemaCollection.url + '/schema';
				schemaCollection.fetch();
				schemaCollection.url = originalUrl;

				var groupedSchemaCollection = new Backbone.Collection();

				schemaCollection.on('sync', function() {

						var groups = schemaCollection.groupBy(function(model, iterator) {
								return model.get('protocol');
						});
						//key is "undefined", which is being placed in the header
						var modelCollection = _.map(groups, function(values, key, list) {
								return {
										//"header"   : key,
										collection : values
								};
						});

						groupedSchemaCollection.reset(modelCollection);
				});

				var view = new AddCollectionView({
						collection       : groupedSchemaCollection,
						targetCollection : collection
				});

				AppLayout.modalRegion.show(view);
		}
};
