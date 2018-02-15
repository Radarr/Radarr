var PagableCollection = require('backbone.pageable');
var ExtraFileModel = require('./ExtraFileModel');
var AsSortedCollection = require('../../../Mixins/AsSortedCollection');

var Collection = PagableCollection.extend({
    url   : window.NzbDrone.ApiRoot + "/extrafile",
    model : ExtraFileModel,

    state : {
        pageSize : 2000,
        sortKey  : 'relativePath',
        order    : -1
    },

    mode : 'client',

    sortMappings : {
        'relativePath'    : {
            sortKey : "relativePath"
        },
        "type" : {
            sortKey : "type"
        },
        "extension" : {
            sortKey : "extension"
        }
    },

    fetchMovieExtras : function(movieId) {
        return this.fetch({ data : { movieId : movieId}});
    }

});

Collection = AsSortedCollection.call(Collection);

module.exports = Collection;
