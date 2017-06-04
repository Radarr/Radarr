var Backbone = require('backbone');
var AlbumModel = require('./AlbumModel');

module.exports = Backbone.Collection.extend({
    model : AlbumModel,

    comparator : function(season) {
        return -season.get('seasonNumber');
    }
});