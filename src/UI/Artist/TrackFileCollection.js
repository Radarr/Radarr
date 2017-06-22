var Backbone = require('backbone');
var TrackFileModel = require('./TrackFileModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/trackfile',
    model : TrackFileModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.artistId = options.artistId;
        this.models = [];
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