var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Backbone = require('backbone');
var Marionette = require('marionette');
var Profiles = require('../Profile/ProfileCollection');
var RootFolders = require('./RootFolders/RootFolderCollection');
var RootFolderLayout = require('./RootFolders/RootFolderLayout');
var ArtistCollection = require('../Artist/ArtistCollection');
var Config = require('../Config');
var Messenger = require('../Shared/Messenger');
var AsValidatedView = require('../Mixins/AsValidatedView');

require('jquery.dotdotdot');

var view = Marionette.ItemView.extend({

    template : 'AddArtist/SearchResultViewTemplate',

    ui : {
        profile               : '.x-profile',
        rootFolder            : '.x-root-folder',
        albumFolder           : '.x-album-folder',
        artistType            : '.x-artist-type',
        monitor               : '.x-monitor',
        monitorTooltip        : '.x-monitor-tooltip',
        addButton             : '.x-add',
        addAlbumButton        : '.x-add-album',
        addSearchButton       : '.x-add-search',
        addAlbumSearchButton  : '.x-add-album-search',
        overview              : '.x-overview'
    },

    events : {
        'click .x-add'                  : '_addWithoutSearch',
        'click .x-add-album'            : '_addWithoutSearch',
        'click .x-add-search'           : '_addAndSearch',
        'click .x-add-album-search'     : '_addAndSearch',
        'change .x-profile'             : '_profileChanged',
        'change .x-root-folder'         : '_rootFolderChanged',
        'change .x-album-folder'        : '_albumFolderChanged',
        'change .x-artist-type'         : '_artistTypeChanged',
        'change .x-monitor'             : '_monitorChanged'
    },

    initialize : function() {

        if (!this.model) {
            throw 'model is required';
        }

        this.templateHelpers = {};
        this._configureTemplateHelpers();

        this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);
        this.listenTo(this.model, 'change', this.render);
        this.listenTo(RootFolders, 'all', this._rootFoldersUpdated);
    },

    onRender : function() {

        var defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);
        var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
        var useAlbumFolder = Config.getValueBoolean(Config.Keys.UseAlbumFolder, true);
        var defaultArtistType = Config.getValue(Config.Keys.DefaultSeriesType, 'standard');
        var defaultMonitorEpisodes = Config.getValue(Config.Keys.MonitorEpisodes, 'missing');

        if (Profiles.get(defaultProfile)) {
            this.ui.profile.val(defaultProfile);
        }

        if (RootFolders.get(defaultRoot)) {
            this.ui.rootFolder.val(defaultRoot);
        }

        this.ui.albumFolder.prop('checked', useAlbumFolder);
        this.ui.artistType.val(defaultArtistType);
        this.ui.monitor.val(defaultMonitorEpisodes);

        //TODO: make this work via onRender, FM?
        //works with onShow, but stops working after the first render
        this.ui.overview.dotdotdot({
            height : 120
        });

        this.templateFunction = Marionette.TemplateCache.get('AddArtist/MonitoringTooltipTemplate');
        var content = this.templateFunction();

        this.ui.monitorTooltip.popover({
            content   : content,
            html      : true,
            trigger   : 'hover',
            title     : 'Track Monitoring Options',
            placement : 'right',
            container : this.$el
        });
    },

    _configureTemplateHelpers : function() {
        var existingArtist = ArtistCollection.where({ foreignArtistId : this.model.get('foreignArtistId') });

        if (existingArtist.length > 0) {
            this.templateHelpers.existing = existingArtist[0].toJSON();
        }

        this.templateHelpers.profiles = Profiles.toJSON();

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

        else if (options.key === Config.Keys.UseAlbumFolder) {
            this.ui.seasonFolder.prop('checked', options.value);
        }

        else if (options.key === Config.Keys.DefaultArtistType) {
            this.ui.artistType.val(options.value);
        }

        else if (options.key === Config.Keys.MonitorEpisodes) {
            this.ui.monitor.val(options.value);
        }
    },

    _profileChanged : function() {
        Config.setValue(Config.Keys.DefaultProfileId, this.ui.profile.val());
    },

    _albumFolderChanged : function() {
        Config.setValue(Config.Keys.UseAlbumFolder, this.ui.albumFolder.prop('checked'));
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

    _artistTypeChanged : function() {
        Config.setValue(Config.Keys.DefaultArtistType, this.ui.artistType.val());
    },

    _monitorChanged : function() {
        Config.setValue(Config.Keys.MonitorEpisodes, this.ui.monitor.val());
    },

    _setRootFolder : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.ui.rootFolder.val(options.model.id);
        this._rootFolderChanged();
    },

    _addWithoutSearch : function(evt) {
        console.log(evt);
        this._addArtist(false);
    },

    _addAndSearch : function() {
        this._addArtist(true);
    },

    _addArtist : function(searchForMissing) {
        // TODO: Refactor to handle multiple add buttons/albums
        var addButton = this.ui.addButton;
        var addSearchButton = this.ui.addSearchButton;
        console.log('_addArtist, searchForMissing=', searchForMissing);

        addButton.addClass('disabled');
        addSearchButton.addClass('disabled');

        var profile = this.ui.profile.val();
        var rootFolderPath = this.ui.rootFolder.children(':selected').text();
        var artistType = this.ui.artistType.val(); // Perhaps make this a differnitator between artist or Album? 
        var albumFolder = this.ui.albumFolder.prop('checked');

        var options = this._getAddArtistOptions();
        options.searchForMissing = searchForMissing;

        this.model.set({
            profileId      : profile,
            rootFolderPath : rootFolderPath,
            albumFolder    : albumFolder,
            artistType     : artistType,
            addOptions     : options,
            monitored      : true
        }, { silent : true });

        var self = this;
        var promise = this.model.save();

        if (searchForMissing) {
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
            console.log('[SearchResultView] _addArtist promise resolve:', self.model);
            ArtistCollection.add(self.model);

            self.close();

            Messenger.show({
                message        : 'Added: ' + self.model.get('name'),
                actions        : {
                    goToArtist : {
                        label  : 'Go to Artist',
                        action : function() {
                            Backbone.history.navigate('/artist/' + self.model.get('nameSlug'), { trigger : true });
                        }
                    }
                },
                hideAfter      : 8,
                hideOnNavigate : true
            });

            vent.trigger(vent.Events.ArtistAdded, { artist : self.model });
        });
    },

    _rootFoldersUpdated : function() {
        this._configureTemplateHelpers();
        this.render();
    },

    _getAddArtistOptions : function() {
        var monitor = this.ui.monitor.val();
        //[TODO]: Refactor for albums
        var lastSeason = _.max(this.model.get('seasons'), 'seasonNumber');
        var firstSeason = _.min(_.reject(this.model.get('seasons'), { seasonNumber : 0 }), 'seasonNumber');

        //this.model.setSeasonPass(firstSeason.seasonNumber); // TODO

        var options = {
            ignoreTracksWithFiles    : false,
            ignoreTracksWithoutFiles : false
        };

        if (monitor === 'all') {
            return options;
        }

        else if (monitor === 'future') {
            options.ignoreTracksWithFiles = true;
            options.ignoreTracksWithoutFiles = true;
        }

        /*else if (monitor === 'latest') {
            this.model.setSeasonPass(lastSeason.seasonNumber);
        }

        else if (monitor === 'first') {
            this.model.setSeasonPass(lastSeason.seasonNumber + 1);
            this.model.setSeasonMonitored(firstSeason.seasonNumber);
        }*/

        else if (monitor === 'missing') {
            options.ignoreTracksWithFiles = true;
        }

        else if (monitor === 'existing') {
            options.ignoreTracksWithoutFiles = true;
        }

        /*else if (monitor === 'none') {
            this.model.setSeasonPass(lastSeason.seasonNumber + 1);
        }*/

        return options;
    }
});

AsValidatedView.apply(view);

module.exports = view;
