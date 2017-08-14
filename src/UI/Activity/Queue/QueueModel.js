var Backbone = require('backbone');
var ArtistModel = require('../../Artist/ArtistModel');
var AlbumModel = require('../../Artist/AlbumModel');

module.exports = Backbone.Model.extend({
    parse : function(model) {
        model.artist = new ArtistModel(model.artist);
        model.album = new AlbumModel(model.album);
        model.album.set('artist', model.artist);
        return model;
    }
});