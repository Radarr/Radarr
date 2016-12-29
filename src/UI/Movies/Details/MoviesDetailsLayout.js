var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backbone = require('backbone');
var MoviesCollection = require('../MoviesCollection');
var InfoView = require('./InfoView');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var EpisodeFileEditorLayout = require('../../EpisodeFile/Editor/EpisodeFileEditorLayout');
require('backstrech');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    itemViewContainer : '.x-movie-seasons',
    template          : 'Movies/Details/MoviesDetailsTemplate',

    regions : {
        seasons : '#seasons',
        info    : '#info'
    },

    ui : {
        header    : '.x-header',
        monitored : '.x-monitored',
        edit      : '.x-edit',
        refresh   : '.x-refresh',
        rename    : '.x-rename',
        search    : '.x-search',
        poster    : '.x-movie-poster',
        manualSearch : '.x-manual-search'
    },

    events : {
        'click .x-episode-file-editor' : '_openEpisodeFileEditor',
        'click .x-monitored'           : '_toggleMonitored',
        'click .x-edit'                : '_editMovies',
        'click .x-refresh'             : '_refreshMovies',
        'click .x-rename'              : '_renameMovies',
        'click .x-search'              : '_moviesSearch',
        'click .x-manual-search'       : '_manualSearchM'
    },

    initialize : function() {
        this.moviesCollection = MoviesCollection.clone();
        this.moviesCollection.shadowCollection.bindSignalR();

        this.listenTo(this.model, 'change:monitored', this._setMonitoredState);
        this.listenTo(this.model, 'remove', this._moviesRemoved);
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
        this._showSeasons();
        this._setMonitoredState();
        this._showInfo();
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name : 'refreshMovies'
            }
        });
        CommandController.bindToCommand({
            element : this.ui.search,
            command : {
                name : 'moviesSearch'
            }
        });

        CommandController.bindToCommand({
            element : this.ui.rename,
            command : {
                name         : 'renameFiles',
                movieId     : this.model.id,
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
        this.ui.monitored.removeClass('fa-spin icon-sonarr-spinner');

        if (monitored) {
            this.ui.monitored.addClass('icon-sonarr-monitored');
            this.ui.monitored.removeClass('icon-sonarr-unmonitored');
            this.$el.removeClass('movie-not-monitored');
        } else {
            this.ui.monitored.addClass('icon-sonarr-unmonitored');
            this.ui.monitored.removeClass('icon-sonarr-monitored');
            this.$el.addClass('movie-not-monitored');
        }
    },

    _editMovies : function() {
        vent.trigger(vent.Commands.EditMoviesCommand, { movie : this.model });
    },

    _refreshMovies : function() {
        CommandController.Execute('refreshMovies', {
            name     : 'refreshMovies',
            movieId : this.model.id
        });
    },

    _moviesRemoved : function() {
        Backbone.history.navigate('/', { trigger : true });
    },

    _renameMovies : function() {
        vent.trigger(vent.Commands.ShowRenamePreview, { movie : this.model });
    },

    _moviesSearch : function() {
        CommandController.Execute('moviesSearch', {
            name     : 'moviesSearch',
            movieId : this.model.id
        });
    },

    _showSeasons : function() {
        var self = this;

        return;

        reqres.setHandler(reqres.Requests.GetEpisodeFileById, function(episodeFileId) {
            return self.episodeFileCollection.get(episodeFileId);
        });

        reqres.setHandler(reqres.Requests.GetAlternateNameBySeasonNumber, function(moviesId, seasonNumber, sceneSeasonNumber) {
            if (self.model.get('id') !== moviesId) {
                return [];
            }

            if (sceneSeasonNumber === undefined) {
                sceneSeasonNumber = seasonNumber;
            }

            return _.where(self.model.get('alternateTitles'),
                function(alt) {
                    return alt.sceneSeasonNumber === sceneSeasonNumber || alt.seasonNumber === seasonNumber;
                });
        });

        $.when(this.episodeCollection.fetch(), this.episodeFileCollection.fetch()).done(function() {
            var seasonCollectionView = new SeasonCollectionView({
                collection        : self.seasonCollection,
                episodeCollection : self.episodeCollection,
                movies            : self.model
            });

            if (!self.isClosed) {
                self.seasons.show(seasonCollectionView);
            }
        });
    },

    _showInfo : function() {
        this.info.show(new InfoView({
            model                 : this.model,
            episodeFileCollection : this.episodeFileCollection
        }));
    },

    _commandComplete : function(options) {
        if (options.command.get('name') === 'renamefiles') {
            if (options.command.get('moviesId') === this.model.get('id')) {
                this._refresh();
            }
        }
    },

    _refresh : function() {
        this.seasonCollection.add(this.model.get('seasons'), { merge : true });
        this.episodeCollection.fetch();
        this.episodeFileCollection.fetch();

        this._setMonitoredState();
        this._showInfo();
    },

    _openEpisodeFileEditor : function() {
        var view = new EpisodeFileEditorLayout({
            movies            : this.model,
            episodeCollection : this.episodeCollection
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
    },

    _manualSearchM : function() {
        console.warn("Manual Search started");
        console.warn(this.model.get("moviesId"));
        console.warn(this.model)
        console.warn(this.episodeCollection);
        vent.trigger(vent.Commands.ShowEpisodeDetails, {
            episode        : this.episodeCollection.models[0],
            hideMoviesLink : true,
            openingTab     : 'search'
        });
    }
});
