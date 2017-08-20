var Backbone = require('backbone');
var AlbumModel = require('./AlbumModel');

module.exports = Backbone.Collection.extend({
	url   : window.NzbDrone.ApiRoot + '/album',
    model : AlbumModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.artistId = options.artistId;
        this.models = [];
    },

    comparator : function(model1, model2) {
        var album1 = model1.get('releaseDate');
        var album2 = model2.get('releaseDate');

        if (album1 > album2) {
            return -1;
        }

        if (album1 < album2) {
            return 1;
        }

        return 0;
    },

    fetch : function(options) {
        if (!this.artistId) {
            throw 'artistId is required';
        }

        if (!options) {
            options = {};
        }

        options.data = { artistId : this.artistId };

        return this.originalFetch.call(this, options);
    }
});