var Backbone = require('backbone');
var ArtistCollection = require('../../Artist/ArtistCollection');

module.exports = Backbone.Model.extend({

    //Hack to deal with Backbone 1.0's bug
    initialize : function() {
        this.url = function() {
            return this.collection.url + '/' + this.get('id');
        };
    },

    parse : function(model) {
        model.artist = ArtistCollection.get(model.artistId);
        return model;
    }
});