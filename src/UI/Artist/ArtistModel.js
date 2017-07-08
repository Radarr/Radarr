var Backbone = require('backbone');
var _ = require('underscore');

module.exports = Backbone.Model.extend({
    urlRoot : window.NzbDrone.ApiRoot + '/artist',

    defaults : {
        trackFileCount : 0,
        trackCount     : 0,
        isExisting       : false,
        status           : 0
    },

    setAlbumsMonitored : function(albumId) {
        _.each(this.get('albums'), function(album) {
            if (album.albumId === albumId) {
                album.monitored = !album.monitored;
            }
        });
    },

    setAlbumPass : function(monitored) {
        _.each(this.get('albums'), function(album) {
            if (monitored === 0) {
                album.monitored = true;
            } else {
                album.monitored = false;
            }
        });
    }
});