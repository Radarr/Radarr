var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
var ArtistModel = require('../../Artist/ArtistModel');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPageableCollection = require('../../Mixins/AsPageableCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var BulkImportCollection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/artist/bulkimport',
    model : ArtistModel,
    tableName : 'bulkimport',

    state : {
        pageSize : 100000,
        sortKey: 'sortName',
        firstPage: 1
    },

    fetch : function(options) {

        options = options || {};

        var data = options.data || {};

        if (!data.id || !data.folder) {
            data.id = this.folderId;
            data.folder = this.folder;
        }

        options.data = data;
        return PageableCollection.prototype.fetch.call(this, options);
    },

    parseLinks : function(options) {

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
