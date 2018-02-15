var PagableCollection = require('backbone.pageable');
var FileModel = require('./FileModel');
var AsSortedCollection = require('../../../Mixins/AsSortedCollection');

var Collection = PagableCollection.extend({
    url   : window.NzbDrone.ApiRoot + "/moviefile",
    model : FileModel,

    state : {
        pageSize : 2000,
        sortKey  : 'title',
        order    : -1
    },

    mode : 'client',

    sortMappings : {
        'quality'    : {
            sortKey : "qualityWeight"
        },
        "edition" : {
          sortKey : "edition"
        }
    },

});

Collection = AsSortedCollection.call(Collection);

module.exports = Collection;
