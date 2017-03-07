var _ = require('underscore');
var MovieModel = require('../../Movies/MovieModel');
var PagableCollection = require('backbone.pageable');
var AsFilteredCollection = require('../../Mixins/AsFilteredCollection');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPersistedStateCollection = require('../../Mixins/AsPersistedStateCollection');

var Collection = PagableCollection.extend({
    url       : window.NzbDrone.ApiRoot + '/wanted/cutoff',
    model     : MovieModel,
    tableName : 'wanted.cutoff',

    state : {
        pageSize : 50,
        sortKey  : 'title',
        order    : -1
    },

    queryParams : {
        totalPages   : null,
        totalRecords : null,
        pageSize     : 'pageSize',
        sortKey      : 'sortKey',
        order        : 'sortDir',
        directions   : {
            '-1' : 'asc',
            '1'  : 'desc'
        }
    },


    filterModes : {
        'monitored'   : [
            'monitored',
            'true'
        ],
        'unmonitored' : [
            'monitored',
            'false'
        ],
        'announced' : [
            'moviestatus',
            'announced'
        ],
        'incinemas' : [
            'moviestatus',
            'incinemas'
        ],
        'released' : [
            'moviestatus',
            'released'
        ],
        'available' : [
            'moviestatus',
            'available'
        ],
        'all' : [
            'all',
            'all'
        ]
    },


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

Collection = AsFilteredCollection.call(Collection);
Collection = AsSortedCollection.call(Collection);

module.exports = AsPersistedStateCollection.call(Collection);