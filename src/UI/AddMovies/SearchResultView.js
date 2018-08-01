var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Backbone = require('backbone');
var Marionette = require('marionette');
var Profiles = require('../Profile/ProfileCollection');
var RootFolders = require('./RootFolders/RootFolderCollection');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var FullMovieCollection = require('../Movies/FullMovieCollection');
var ImportExclusionModel = require("../Settings/NetImport/ImportExclusionModel");
var Config = require('../Config');
var Messenger = require('../Shared/Messenger');
var AsValidatedView = require('../Mixins/AsValidatedView');

require('jquery.dotdotdot');

var view = Marionette.ItemView.extend({

    template : 'AddMovies/SearchResultViewTemplate',

    ui : {
        profile         : '.x-profile',
        rootFolder      : '.x-root-folder',
        seasonFolder    : '.x-season-folder',
        monitor         : '.x-monitor',
	      minimumAvailability : '.x-minimumavailability',
	      minimumAvailabilityTooltip : '.x-minimumavailability-tooltip',
        monitorTooltip  : '.x-monitor-tooltip',
        addButton       : '.x-add',
        addSearchButton : '.x-add-search',
        overview        : '.x-overview'
    },

    events : {
        'click .x-add'            : '_addWithoutSearch',
        'click .x-add-search'     : '_addAndSearch',
        "click .x-ignore"         : "_ignoreMovie",
        'change .x-profile'       : '_profileChanged',
        'change .x-root-folder'   : '_rootFolderChanged',
        'change .x-season-folder' : '_seasonFolderChanged',
        "change .x-minimumavailability" : "_minAvailabilityChanged",
        'change .x-monitor'       : '_monitorChanged'
    },

    initialize : function() {

        if (!this.model) {
            throw 'model is required';
        }

        //console.log(this.route);

        this.templateHelpers = {};
        this._configureTemplateHelpers();

        this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
        this.listenTo(this.model, 'change', this.render);
        this.listenTo(RootFolders, 'all', this._rootFoldersUpdated);
    },

    onRender : function() {

        var defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);
        var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
        var defaultMinAvailability = Config.getValue(Config.Keys.DefaultMinAvailability, "announced");
        var useSeasonFolder = Config.getValueBoolean(Config.Keys.UseSeasonFolder, true);
        var defaultMonitorEpisodes = Config.getValue(Config.Keys.MonitorEpisodes, 'all');

        if (Profiles.get(defaultProfile)) {
            this.ui.profile.val(defaultProfile);
        }

        if (RootFolders.get(defaultRoot)) {
            this.ui.rootFolder.val(defaultRoot);
        }

        this.ui.seasonFolder.prop('checked', useSeasonFolder);
        this.ui.monitor.val(defaultMonitorEpisodes);
	      this.ui.minimumAvailability.val(defaultMinAvailability);

        //TODO: make this work via onRender, FM?
        //works with onShow, but stops working after the first render
        this.ui.overview.dotdotdot({
            height : 120
        });

        this.templateFunction = Marionette.TemplateCache.get('AddMovies/MonitoringTooltipTemplate');
        var content = this.templateFunction();

        this.ui.monitorTooltip.popover({
            content   : content,
            html      : true,
            trigger   : 'hover',
            title     : 'Movie Monitoring Options',
            placement : 'right',
            container : this.$el
        });

	this.templateFunction = Marionette.TemplateCache.get('AddMovies/MinimumAvailabilityTooltipTemplate');
	var content1 = this.templateFunction();

	this.ui.minimumAvailabilityTooltip.popover({
		content : content1,
		html :true,
		trigger : 'hover',
		title : 'When to Consider a Movie Available',
		placement : 'right',
		container : this.$el
	});
    },

    _configureTemplateHelpers : function() {
        var existingMovies = FullMovieCollection.where({ tmdbId : this.model.get('tmdbId') });
        if (existingMovies.length > 0) {
            this.templateHelpers.existing = existingMovies[0].toJSON();
        }

        this.templateHelpers.profiles = Profiles.toJSON();
        //console.log(this.templateHelpers.isExisting);
        if (!this.model.get('isExisting')) {
            this.templateHelpers.rootFolders = RootFolders.toJSON();
        }
    },

    _onConfigUpdated : function(options) {
        if (options.key === Config.Keys.DefaultProfileId) {
            this.ui.profile.val(options.value);
        }

        else if (options.key === Config.Keys.DefaultRootFolderId) {
            this.ui.rootFolder.val(options.value);
        }

        else if (options.key === Config.Keys.UseSeasonFolder) {
            this.ui.seasonFolder.prop('checked', options.value);
        }

        else if (options.key === Config.Keys.MonitorEpisodes) {
            this.ui.monitor.val(options.value);
        }

        else if (options.key === Config.Keys.DefaultMinAvailability) {
            this.ui.minimumAvailability.val(options.value);
        }
    },

    _profileChanged : function() {
        Config.setValue(Config.Keys.DefaultProfileId, this.ui.profile.val());
    },

    _seasonFolderChanged : function() {
        Config.setValue(Config.Keys.UseSeasonFolder, this.ui.seasonFolder.prop('checked'));
    },

    _rootFolderChanged : function() {
        var rootFolderValue = this.ui.rootFolder.val();
        if (rootFolderValue === 'addNew') {
            var rootFolderLayout = new RootFolderLayout();
            this.listenToOnce(rootFolderLayout, 'folderSelected', this._setRootFolder);
            AppLayout.modalRegion.show(rootFolderLayout);
        } else {
            Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
        }
    },

    _monitorChanged : function() {
        Config.setValue(Config.Keys.MonitorEpisodes, this.ui.monitor.val());
    },

    _minAvailabilityChanged : function() {
        Config.setValue(Config.Keys.DefaultMinAvailability, this.ui.minimumAvailability.val());
    },

    _setRootFolder : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.ui.rootFolder.val(options.model.id);
        this._rootFolderChanged();
    },

    _addWithoutSearch : function() {
        this._addMovies(false);
    },

    _addAndSearch : function() {
        this._addMovies(true);
    },

    _addMovies : function(searchForMovie) {
        var addButton = this.ui.addButton;
        var addSearchButton = this.ui.addSearchButton;

        addButton.addClass('disabled');
        addSearchButton.addClass('disabled');

        var profile = this.ui.profile.val();
        var rootFolderPath = this.ui.rootFolder.children(':selected').text();
        var monitor = this.ui.monitor.val();
        var minAvail = this.ui.minimumAvailability.val();

        var options = this._getAddMoviesOptions();
        options.searchForMovie = searchForMovie;
        console.warn(searchForMovie);

        this.model.set({
            profileId      : profile,
            rootFolderPath : rootFolderPath,
            addOptions     : options,
	    minimumAvailability : minAvail,
            monitored      : (monitor === 'all' ? true : false)
        }, { silent : true });

        var self = this;
        var promise = this.model.save();

        //console.log(this.model.save);
        //console.log(promise);

        if (searchForMovie) {
            this.ui.addSearchButton.spinForPromise(promise);
        }

        else {
            this.ui.addButton.spinForPromise(promise);
        }

        promise.always(function() {
            addButton.removeClass('disabled');
            addSearchButton.removeClass('disabled');
        });

        promise.done(function() {
            FullMovieCollection.add(self.model);

            self.close();

            Messenger.show({
                message        : 'Added: ' + self.model.get('title'),
                actions        : {
                    goToMovie : {
                        label  : 'Go to Movie',
                        action : function() {
                            Backbone.history.navigate('/movies/' + self.model.get('titleSlug'), { trigger : true });
                        }
                    }
                },
                hideAfter      : 8,
                hideOnNavigate : true
            });

            vent.trigger(vent.Events.MoviesAdded, { movie : self.model });
        });
    },

    _ignoreMovie : function() {
      var exclusion = new ImportExclusionModel({tmdbId : this.model.get("tmdbId"),
        movieTitle : this.model.get("title"), movieYear : this.model.get("year")});
      exclusion.save();
      this.model.destroy();
      this.remove();
    },

    _rootFoldersUpdated : function() {
        this._configureTemplateHelpers();
        this.render();
    },

    _getAddMoviesOptions : function() {
        return {
            ignoreEpisodesWithFiles    : false,
            ignoreEpisodesWithoutFiles : false
        };
    }
});

AsValidatedView.apply(view);

module.exports = view;
