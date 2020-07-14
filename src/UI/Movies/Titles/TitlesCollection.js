var PagableCollection = require('backbone.pageable');
var TitleModel = require('./TitleModel');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');

var Collection = PagableCollection.extend({
    url   : window.NzbDrone.ApiRoot + "/aka",
    model : TitleModel,

    state : {
        pageSize : 2000,
        sortKey  : 'title',
        order    : -1
    },

    mode : 'client',

    sortMappings : {
        "source" : {
            sortKey : "sourceType"
        },
        "language" : {
            sortKey : "language"
        }
    },

});

Collection = AsSortedCollection.call(Collection);

module.exports = Collection;
