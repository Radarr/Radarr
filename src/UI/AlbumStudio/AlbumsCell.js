var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var TemplatedCell = require('../Cells/TemplatedCell');
var AlbumCollection = require('../Artist/AlbumCollection');
var LoadingView = require('../Shared/LoadingView');
var ArtistCollection = require('../Artist/ArtistCollection');
var AlbumCollectionView = require('./AlbumStudioCollectionView');
//require('../Handlebars/Helpers/Numbers');

module.exports = Marionette.Layout.extend({
    template  : 'AlbumStudio/AlbumsCellTemplate',
    tagName   : 'td',

    regions : {
        albums : '#albums'
    },

    initialize : function() {
        this.artistCollection = ArtistCollection.clone();
        this.artistCollection.shadowCollection.bindSignalR();

        this.listenTo(this.model, 'change:monitored', this._setMonitoredState);
        this.listenTo(this.model, 'remove', this._artistRemoved);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);

        this.listenTo(this.model, 'change', function(model, options) {
            if (options && options.changeSource === 'signalr') {
                this._refresh();
            }
        });
    },

    onRender : function(){
        this._showAlbums();
    },

    _showAlbums : function() {
        var self = this;

        this.albums.show(new LoadingView());

        this.albumCollection = new AlbumCollection({ artistId : this.model.id }).bindSignalR();

        $.when(this.albumCollection.fetch()).done(function() {
            var albumCollectionView = new AlbumCollectionView({
                collection        : self.albumCollection,
                artist            : self.model
            });

            if (!self.isClosed) {
                self.albums.show(albumCollectionView);
            }
        });
    },


});