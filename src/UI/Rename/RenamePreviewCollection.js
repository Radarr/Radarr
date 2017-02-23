var Backbone = require('backbone');
var RenamePreviewModel = require('./RenamePreviewModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/renameMovie',
    model : RenamePreviewModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        if (!options.movieId) {
            throw 'movieId is required';
        }

        this.movieId = options.movieId;
        //this.seasonNumber = options.seasonNumber;
    },

    fetch : function(options) {
        if (!this.movieId) {
            throw 'movieId is required';
        }

        options = options || {};
        options.data = {};
        options.data.movieId = this.movieId;

        // if (this.seasonNumber !== undefined) {
        //     options.data.seasonNumber = this.seasonNumber;
        //}

        return this.originalFetch.call(this, options);
    }
});