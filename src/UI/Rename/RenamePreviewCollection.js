var Backbone = require('backbone');
var RenamePreviewModel = require('./RenamePreviewModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/rename',
    model : RenamePreviewModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        if (!options.artistId) {
            throw 'artistId is required';
        }

        this.artistId = options.artistId;
        this.albumId = options.albumId;
    },

    fetch : function(options) {
        if (!this.artistId) {
            throw 'artistId is required';
        }

        options = options || {};
        options.data = {};
        options.data.artistId = this.artistId;

        if (this.albumId !== undefined) {
            options.data.albumId = this.albumId;
        }

        return this.originalFetch.call(this, options);
    }
});