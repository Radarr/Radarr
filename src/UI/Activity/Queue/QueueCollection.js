var _ = require('underscore');
var PageableCollection = require('backbone.pageable');
//var PageableCollection = require('../../Shared/Grid/LidarrPageableCollection');
var QueueModel = require('./QueueModel');
var FormatHelpers = require('../../Shared/FormatHelpers');
var AsSortedCollection = require('../../Mixins/AsSortedCollection');
var AsPageableCollection = require('../../Mixins/AsPageableCollection');
var moment = require('moment');

require('../../Mixins/backbone.signalr.mixin');

var QueueCollection = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/queue',
    model : QueueModel,

    state : {
        pageSize : 15,
        sortKey: 'timeleft'
    },

    mode : 'client',

    findEpisode : function(albumId) {
        return _.find(this.fullCollection.models, function(queueModel) {
            return queueModel.get('album').id === albumId;
        });
    },

    sortMappings : {
        artist : {
            sortValue : function(model, attr) {
                var artist = model.get(attr);

                return artist.get('sortName');
            }
        },

        albumTitle : {
            sortValue : function(model, attr) {
                var album = model.get('album');

                return album.get('title');
            }
        },

        timeleft : {
            sortValue : function(model, attr) {
                var eta = model.get('estimatedCompletionTime');

                if (eta) {
                    return moment(eta).unix();
                }

                return Number.MAX_VALUE;
            }
        },

        sizeleft : {
            sortValue : function(model, attr) {
                var size = model.get('size');
                var sizeleft = model.get('sizeleft');

                if (size && sizeleft) {
                    return sizeleft / size;
                }

                return 0;
            }
        }
    }
});

QueueCollection = AsSortedCollection.call(QueueCollection);
QueueCollection = AsPageableCollection.call(QueueCollection);

var collection = new QueueCollection().bindSignalR();
collection.fetch();

module.exports = collection;