var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backbone = require('backbone');
var ArtistCollection = require('../ArtistCollection');
var TrackCollection = require('../TrackCollection');
var TrackFileCollection = require('../TrackFileCollection');
var AlbumCollection = require('../AlbumCollection');
var AlbumCollectionView = require('./AlbumCollectionView');
var InfoView = require('./InfoView');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var TrackFileEditorLayout = require('../../EpisodeFile/Editor/EpisodeFileEditorLayout');
require('backstrech');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    itemViewContainer : '.x-artist-albums',
    template          : 'Artist/Details/ArtistDetailsTemplate',

    regions : {
        albums : '#albums',
        info    : '#info'
    },

    ui : {
        header    : '.x-header',
        monitored : '.x-monitored',
        edit      : '.x-edit',
        refresh   : '.x-refresh',
        rename    : '.x-rename',
        search    : '.x-search',
        poster    : '.x-artist-poster'
    },

    events : {
        'click .x-track-file-editor'   : '_openTrackFileEditor',
        'click .x-monitored'           : '_toggleMonitored',
        'click .x-edit'                : '_editArtist',
        'click .x-refresh'             : '_refreshArtist',
        'click .x-rename'              : '_renameArtist',
        'click .x-search'              : '_artistSearch'
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

        this.listenTo(this.model,  'change:images', this._updateImages);
    },

    onShow : function() {
        this._showBackdrop();
        this._showAlbums();
        this._setMonitoredState();
        this._showInfo();
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name : 'refreshArtist'
            }
        });
        CommandController.bindToCommand({
            element : this.ui.search,
            command : {
                name : 'artistSearch'
            }
        });

        CommandController.bindToCommand({
            element : this.ui.rename,
            command : {
                name         : 'renameFiles',
                seriesId     : this.model.id,
                seasonNumber : -1
            }
        });
    },

    onClose : function() {
        if (this._backstrech) {
            this._backstrech.destroy();
            delete this._backstrech;
        }

        $('body').removeClass('backdrop');
        reqres.removeHandler(reqres.Requests.GetEpisodeFileById);
    },

    _getImage : function(type) {
        var image = _.where(this.model.get('images'), { coverType : type });

        if (image && image[0]) {
            return image[0].url;
        }

        return undefined;
    },

    _toggleMonitored : function() {
        var savePromise = this.model.save('monitored', !this.model.get('monitored'), { wait : true });

        this.ui.monitored.spinForPromise(savePromise);
    },

    _setMonitoredState : function() {
        var monitored = this.model.get('monitored');

        this.ui.monitored.removeAttr('data-idle-icon');
        this.ui.monitored.removeClass('fa-spin icon-lidarr-spinner');

        if (monitored) {
            this.ui.monitored.addClass('icon-lidarr-monitored');
            this.ui.monitored.removeClass('icon-lidarr-unmonitored');
            this.$el.removeClass('series-not-monitored');
        } else {
            this.ui.monitored.addClass('icon-lidarr-unmonitored');
            this.ui.monitored.removeClass('icon-lidarr-monitored');
            this.$el.addClass('series-not-monitored');
        }
    },

    _editArtist : function() {
        vent.trigger(vent.Commands.EditArtistCommand, { artist : this.model });
    },

    _refreshArtist : function() {
        CommandController.Execute('refreshArtist', {
            name     : 'refreshArtist',
            artistId : this.model.id
        });
    },

    _artistRemoved : function() {
        Backbone.history.navigate('/', { trigger : true });
    },

    _renameArtist : function() {
        vent.trigger(vent.Commands.ShowRenamePreview, { artist : this.model });
    },

    _artistSearch : function() {
        console.log('_artistSearch:', this.model);
        CommandController.Execute('artistSearch', {
            name     : 'artistSearch',
            artistId : this.model.id
        });
    },

    _showAlbums : function() {
        var self = this;

        this.albums.show(new LoadingView());

        this.albumCollection = new AlbumCollection({ artistId : this.model.id }).bindSignalR();

        this.trackCollection = new TrackCollection({ artistId : this.model.id }).bindSignalR();
        this.trackFileCollection = new TrackFileCollection({ artistId : this.model.id }).bindSignalR();

        console.log (this.trackCollection);

        reqres.setHandler(reqres.Requests.GetEpisodeFileById, function(trackFileId) {
            return self.trackFileCollection.get(trackFileId);
        });


        $.when(this.albumCollection.fetch(), this.trackCollection.fetch(), this.trackFileCollection.fetch()).done(function() {
            var albumCollectionView = new AlbumCollectionView({
                collection        : self.albumCollection,
                trackCollection   : self.trackCollection,
                artist            : self.model
            });

            if (!self.isClosed) {
                self.albums.show(albumCollectionView);
            }
        });
    },

    _showInfo : function() {
        this.info.show(new InfoView({
            model                 : this.model,
            trackFileCollection : this.trackFileCollection
        }));
    },

    _commandComplete : function(options) {
        if (options.command.get('name') === 'renamefiles') {
            if (options.command.get('artistId') === this.model.get('id')) {
                this._refresh();
            }
        }
    },

    _refresh : function() {
        this.albumCollection.fetch();
        this.trackCollection.fetch();
        this.trackFileCollection.fetch();

        this._setMonitoredState();
        this._showInfo();
    },

    _openTrackFileEditor : function() {
        var view = new TrackFileEditorLayout({
            artist            : this.model,
            trackCollection   : this.trackCollection
        });

        vent.trigger(vent.Commands.OpenModalCommand, view);
    },

    _updateImages : function () {
        var poster = this._getImage('poster');

        if (poster) {
            this.ui.poster.attr('src', poster);
        }

        this._showBackdrop();
    },

    _showBackdrop : function () {
        $('body').addClass('backdrop');
        var fanArt = this._getImage('fanart');

        if (fanArt) {
            this._backstrech = $.backstretch(fanArt);
        } else {
            $('body').removeClass('backdrop');
        }
    }
});