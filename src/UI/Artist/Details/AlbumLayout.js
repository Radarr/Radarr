var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ToggleCell = require('../../Cells/EpisodeMonitoredCell');
var EpisodeTitleCell = require('../../Cells/EpisodeTitleCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var EpisodeStatusCell = require('../../Cells/EpisodeStatusCell');
var EpisodeActionsCell = require('../../Cells/EpisodeActionsCell');
var TrackNumberCell = require('./TrackNumberCell');
var TrackWarningCell = require('./TrackWarningCell');
var CommandController = require('../../Commands/CommandController');
var EpisodeFileEditorLayout = require('../../EpisodeFile/Editor/EpisodeFileEditorLayout');
var moment = require('moment');
var _ = require('underscore');
var Messenger = require('../../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    template : 'Artist/Details/ArtistLayoutTemplate',

    ui : {
        seasonSearch    : '.x-album-search',
        seasonMonitored : '.x-album-monitored',
        seasonRename    : '.x-album-rename'
    },

    events : {
        'click .x-album-episode-file-editor' : '_openEpisodeFileEditor',
        'click .x-album-monitored'           : '_albumMonitored',
        'click .x-album-search'              : '_albumSearch',
        'click .x-album-rename'              : '_albumRename',
        'click .x-show-hide-episodes'        : '_showHideEpisodes',
        'dblclick .artist-album h2'          : '_showHideEpisodes'
    },

    regions : {
        episodeGrid : '.x-episode-grid'
    },

    columns : [
        {
            name       : 'monitored',
            label      : '',
            cell       : ToggleCell,
            trueClass  : 'icon-lidarr-monitored',
            falseClass : 'icon-lidarr-unmonitored',
            tooltip    : 'Toggle monitored status',
            sortable   : false
        },
        {
            name  : 'trackNumber',
            label : '#',
            cell  : TrackNumberCell
        },
        {
            name      : 'this',
            label     : '',
            cell      : TrackWarningCell,
            sortable  : false,
            className : 'track-warning-cell'
        },
        {
            name           : 'this',
            label          : 'Title',
            hideSeriesLink : true,
            cell           : EpisodeTitleCell,
            sortable       : false
        },
        {
            name  : 'airDateUtc',
            label : 'Air Date',
            cell  : RelativeDateCell
        },
        {
            name     : 'status',
            label    : 'Status',
            cell     : EpisodeStatusCell,
            sortable : false
        },
        {
            name     : 'this',
            label    : '',
            cell     : EpisodeActionsCell,
            sortable : false
        }
    ],

    templateHelpers : function() {
        var episodeCount = this.episodeCollection.filter(function(episode) {
            return episode.get('hasFile') || episode.get('monitored') && moment(episode.get('airDateUtc')).isBefore(moment());
        }).length;

        var episodeFileCount = this.episodeCollection.where({ hasFile : true }).length;
        var percentOfEpisodes = 100;

        if (episodeCount > 0) {
            percentOfEpisodes = episodeFileCount / episodeCount * 100;
        }

        return {
            showingEpisodes   : this.showingEpisodes,
            episodeCount      : episodeCount,
            episodeFileCount  : episodeFileCount,
            percentOfEpisodes : percentOfEpisodes
        };
    },

    initialize : function(options) {
        if (!options.episodeCollection) {
            throw 'episodeCollection is required';
        }

        this.series = options.series;
        this.fullEpisodeCollection = options.episodeCollection;
        this.episodeCollection = this.fullEpisodeCollection.bySeason(this.model.get('seasonNumber'));
        this._updateEpisodeCollection();

        this.showingEpisodes = this._shouldShowEpisodes();

        this.listenTo(this.model, 'sync', this._afterSeasonMonitored);
        this.listenTo(this.episodeCollection, 'sync', this.render);

        this.listenTo(this.fullEpisodeCollection, 'sync', this._refreshEpisodes);
    },

    onRender : function() {
        if (this.showingEpisodes) {
            this._showEpisodes();
        }

        this._setSeasonMonitoredState();

        CommandController.bindToCommand({
            element : this.ui.seasonSearch,
            command : {
                name         : 'seasonSearch',
                seriesId     : this.series.id,
                seasonNumber : this.model.get('seasonNumber')
            }
        });

        CommandController.bindToCommand({
            element : this.ui.seasonRename,
            command : {
                name         : 'renameFiles',
                seriesId     : this.series.id,
                seasonNumber : this.model.get('seasonNumber')
            }
        });
    },

    _seasonSearch : function() {
        CommandController.Execute('seasonSearch', {
            name         : 'seasonSearch',
            seriesId     : this.series.id,
            seasonNumber : this.model.get('seasonNumber')
        });
    },

    _seasonRename : function() {
        vent.trigger(vent.Commands.ShowRenamePreview, {
            series       : this.series,
            seasonNumber : this.model.get('seasonNumber')
        });
    },

    _seasonMonitored : function() {
        if (!this.series.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when series is not monitored',
                type    : 'error'
            });

            return;
        }

        var name = 'monitored';
        this.model.set(name, !this.model.get(name));
        this.series.setSeasonMonitored(this.model.get('seasonNumber'));

        var savePromise = this.series.save().always(this._afterSeasonMonitored.bind(this));

        this.ui.seasonMonitored.spinForPromise(savePromise);
    },

    _afterSeasonMonitored : function() {
        var self = this;

        _.each(this.episodeCollection.models, function(episode) {
            episode.set({ monitored : self.model.get('monitored') });
        });

        this.render();
    },

    _setSeasonMonitoredState : function() {
        this.ui.seasonMonitored.removeClass('icon-lidarr-spinner fa-spin');

        if (this.model.get('monitored')) {
            this.ui.seasonMonitored.addClass('icon-lidarr-monitored');
            this.ui.seasonMonitored.removeClass('icon-lidarr-unmonitored');
        } else {
            this.ui.seasonMonitored.addClass('icon-lidarr-unmonitored');
            this.ui.seasonMonitored.removeClass('icon-lidarr-monitored');
        }
    },

    _showEpisodes : function() {
        this.episodeGrid.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : this.episodeCollection,
            className  : 'table table-hover season-grid'
        }));
    },

    _shouldShowEpisodes : function() {
        var startDate = moment().add('month', -1);
        var endDate = moment().add('year', 1);

        return this.episodeCollection.some(function(episode) {
            var airDate = episode.get('airDateUtc');

            if (airDate) {
                var airDateMoment = moment(airDate);

                if (airDateMoment.isAfter(startDate) && airDateMoment.isBefore(endDate)) {
                    return true;
                }
            }

            return false;
        });
    },

    _showHideEpisodes : function() {
        if (this.showingEpisodes) {
            this.showingEpisodes = false;
            this.episodeGrid.close();
        } else {
            this.showingEpisodes = true;
            this._showEpisodes();
        }

        this.templateHelpers.showingEpisodes = this.showingEpisodes;
        this.render();
    },

    _episodeMonitoredToggled : function(options) {
        var model = options.model;
        var shiftKey = options.shiftKey;

        if (!this.episodeCollection.get(model.get('id'))) {
            return;
        }

        if (!shiftKey) {
            return;
        }

        var lastToggled = this.episodeCollection.lastToggled;

        if (!lastToggled) {
            return;
        }

        var currentIndex = this.episodeCollection.indexOf(model);
        var lastIndex = this.episodeCollection.indexOf(lastToggled);

        var low = Math.min(currentIndex, lastIndex);
        var high = Math.max(currentIndex, lastIndex);
        var range = _.range(low + 1, high);

        this.episodeCollection.lastToggled = model;
    },

    _updateEpisodeCollection : function() {
        var self = this;

        this.episodeCollection.add(this.fullEpisodeCollection.bySeason(this.model.get('seasonNumber')).models, { merge : true });

        this.episodeCollection.each(function(model) {
            model.episodeCollection = self.episodeCollection;
        });
    },

    _refreshEpisodes : function() {
        this._updateEpisodeCollection();
        this.episodeCollection.fullCollection.sort();
        this.render();
    },

    _openEpisodeFileEditor : function() {
        var view = new EpisodeFileEditorLayout({
            model             : this.model,
            series            : this.series,
            episodeCollection : this.episodeCollection
        });

        vent.trigger(vent.Commands.OpenModalCommand, view);
    }
});