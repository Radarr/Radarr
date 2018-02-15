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
var HistoryLayout = require('../History/MovieHistoryLayout');
var SearchLayout = require('../Search/MovieSearchLayout');
var AllFilesLayout = require("../Files/AllFilesLayout");
var TitlesLayout = require("../Titles/TitlesLayout");
require('backstrech');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
		itemViewContainer : '.x-movie-seasons',
		template          : 'Movies/Details/MoviesDetailsTemplate',

		regions : {
				info    	: '#info',
				search  	: '#movie-search',
				history 	: '#movie-history',
				filesTabs	: '#movie-files-tabs',
				titles  	: "#movie-titles",
		},


		ui : {
				header    	: '.x-header',
				monitored 	: '.x-monitored',
				edit      	: '.x-edit',
				refresh   	: '.x-refresh',
				rename    	: '.x-rename',
				searchAuto  : '.x-search',
				poster    	: '.x-movie-poster',
				manualSearch: '.x-manual-search',
				history   	: '.x-movie-history',
				search    	: '.x-movie-search',
				filesTabs 	: '.x-movie-files-tabs',
				titles    	: ".x-movie-titles",
		},

		events : {
				'click .x-episode-file-editor' : '_showFiles',
				'click .x-monitored'           : '_toggleMonitored',
				'click .x-edit'                : '_editMovie',
				'click .x-refresh'             : '_refreshMovies',
				'click .x-rename'              : '_renameMovies',
				'click .x-search'              : '_moviesSearch',
				'click .x-manual-search'       : '_showSearch',
				'click .x-movie-history'       : '_showHistory',
				'click .x-movie-search'        : '_showSearch',
				'click .x-movie-files-tabs'    : '_showFileTabs',
				"click .x-movie-titles"        : "_showTitles",
		},

		initialize : function() {
				this.moviesCollection = MoviesCollection.clone();
				this.moviesCollection.bindSignalR();

				this.listenTo(this.model, 'change:monitored', this._setMonitoredState);
				this.listenTo(this.model, 'remove', this._moviesRemoved);
				this.listenTo(this.model, "change:movieFile", this._refreshFiles);

				this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);

				this.listenTo(this.model, 'change', function(model, options) {
						if (options && options.changeSource === 'signalr') {
								this._refresh();
						}
				});

				this.listenTo(this.model,  'change:images', this._updateImages);
		},

		_refreshFiles : function() {
			this._showFileTabs();
		},

		onShow : function() {
				this.searchLayout = new SearchLayout({ model : this.model });
				this.searchLayout.startManualSearch = true;
				this.allFilesLayout = new AllFilesLayout({ model : this.model });
            	this.titlesLayout = new TitlesLayout({ model : this.model });

				this._showBackdrop();
				this._showSeasons();
				this._setMonitoredState();
				this._showInfo();
				this._showHistory();
		},

		onRender : function() {
				CommandController.bindToCommand({
						element : this.ui.refresh,
						command : {
								name : 'refreshMovie'
						}
				});

				CommandController.bindToCommand({
						element : this.ui.searchAuto,
						command : {
								name : 'moviesSearch'
						}
				});

				CommandController.bindToCommand({
						element : this.ui.rename,
						command : {
								name         : 'renameMovieFiles',
								movieId      : this.model.id,
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

		_showHistory : function(e) {
				if (e) {
						e.preventDefault();
				}

				this.ui.history.tab('show');
				this.history.show(new HistoryLayout({
						model  : this.model
				}));
		},

		_showSearch : function(e) {
				if (e) {
						e.preventDefault();
				}

				this.ui.search.tab('show');
				this.search.show(this.searchLayout);
		},

		_showFileTabs : function(e) {
			if (e) {
				e.preventDefault();
			}

			this.ui.filesTabs.tab('show');
			this.filesTabs.show(this.allFilesLayout);
		},

		_showTitles : function(e) {
            if (e) {
                e.preventDefault();
            }

            this.ui.titles.tab("show");
            this.titles.show(this.titlesLayout);
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

		_editMovie : function() {
				vent.trigger(vent.Commands.EditMovieCommand, { movie : this.model });
		},

		_refreshMovies : function() {
				CommandController.Execute('refreshMovie', {
						name     : 'refreshMovie',
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
						movieIds : [this.model.id]
				});
		},

		_showSeasons : function() {
				var self = this;

				return;
		},

		_showInfo : function() {
				this.info.show(new InfoView({
						model                 : this.model
				}));
		},

		_commandComplete : function(options) {
				if (options.command.get('name') === 'renameMoviefiles') {
						if (options.command.get('moviesId') === this.model.get('id')) {
								this._refresh();
						}
				}
		},

		_refresh : function() {
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
				var fanArt = this._getImage('banner');

				if (fanArt) {
						this._backstrech = $.backstretch(fanArt);
				} else {
						$('body').removeClass('backdrop');
				}
		},

		_manualSearchM : function() {
				console.warn("Manual Search started");
				console.warn(this.model.id);
				console.warn(this.model);
				console.warn(this.episodeCollection);
				vent.trigger(vent.Commands.ShowEpisodeDetails, {
						episode        : this.model,
						hideMoviesLink : true,
						openingTab     : 'search'
				});
		}
});
