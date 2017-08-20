var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ToggleCell = require('../../Cells/TrackMonitoredCell');
var TrackTitleCell = require('../../Cells/TrackTitleCell');
var TrackExplicitCell = require('../../Cells/TrackExplicitCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var TrackStatusCell = require('../../Cells/TrackStatusCell');
var TrackActionsCell = require('../../Cells/TrackActionsCell');
var TrackNumberCell = require('./TrackNumberCell');
var TrackWarningCell = require('./TrackWarningCell');
var TrackRatingCell = require('./TrackRatingCell');
var TrackDurationCell = require('../../Cells/TrackDurationCell');
var AlbumInfoView = require('./AlbumInfoView');
var CommandController = require('../../Commands/CommandController');
//var TrackFileEditorLayout = require('../../TrackFile/Editor/TrackFileEditorLayout');
var moment = require('moment');
var _ = require('underscore');
var Messenger = require('../../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    template : 'Artist/Details/AlbumLayoutTemplate',

    ui : {
        albumSearch    : '.x-album-search',
        albumMonitored : '.x-album-monitored',
        albumRename    : '.x-album-rename',
        albumDetails   : '.x-album-details',
        cover          : '.x-album-cover'
    },

    events : {
        'click .x-track-file-editor'         : '_openTrackFileEditor',
        'click .x-album-monitored'           : '_albumMonitored',
        'click .x-album-search'              : '_albumSearch',
        'click .x-album-rename'              : '_albumRename',
        'click .x-album-details'             : '_albumDetails',
        'click .x-show-hide-tracks'          : '_showHideTracks',
        'dblclick .artist-album h2'          : '_showHideTracks'
    },

    regions : {
        trackGrid : '.x-track-grid',
        albumInfo      : '#album-info'
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
            hideArtistLink : true,
            cell           : TrackTitleCell,
            sortable       : false
        },
        {
            name  : 'this',
            label : 'Rating',
            cell  : TrackRatingCell
        },
        {
            name  : 'this',
            label : 'Content',
            cell  : TrackExplicitCell
        },
        //{
        //    name  : 'airDateUtc',
        //    label : 'Air Date',
        //    cell  : RelativeDateCell
        //},
        {
            name     : 'duration',
            label    : 'Duration',
            cell     : TrackDurationCell,
            sortable : false
        },
        {
            name     : 'status',
            label    : 'Status',
            cell     : TrackStatusCell,
            sortable : false
        }
        //{
        //   name     : 'this',
        //    label    : '',
        //    cell     : TrackActionsCell,
        //    sortable : false
        //} 
    ],

    templateHelpers : function() {
        var trackCount = this.trackCollection.filter(function(track) {
            return track.get('hasFile') || track.get('monitored');
        }).length;

        var trackFileCount = this.trackCollection.where({ hasFile : true }).length;
        var percentOfTracks = 100;

        if (trackCount > 0) {
            percentOfTracks = trackFileCount / trackCount * 100;
        }

        return {
            showingTracks   : this.showingTracks,
            trackCount      : trackCount,
            trackFileCount  : trackFileCount,
            percentOfTracks : percentOfTracks
        };
    },

    initialize : function(options) {
        if (!options.trackCollection) {
            throw 'trackCollection is required';
        }
        
        this.artist = options.artist;
        this.fullTrackCollection = options.trackCollection;
        
        this.trackCollection = this.fullTrackCollection.byAlbum(this.model.get('id'));
        this._updateTrackCollection();

        this.showingTracks = this._shouldShowTracks();

        this.listenTo(this.model, 'sync', this._afterAlbumMonitored);
        this.listenTo(this.trackCollection, 'sync', this.render);
        this.listenTo(this.fullTrackCollection, 'sync', this._refreshTracks);
        this.listenTo(this.model,  'change:images', this._updateImages);
    },

    onRender : function() {
        if (this.showingTracks) {
            this._showTracks();
        }

        this._showAlbumInfo();

        this._setAlbumMonitoredState();

        CommandController.bindToCommand({
            element : this.ui.albumSearch,
            command : {
                name         : 'albumSearch',
                artistId     : this.artist.id,
                albumIds : [this.model.get('id')]
            }
        });

        CommandController.bindToCommand({
            element : this.ui.albumRename,
            command : {
                name         : 'renameFiles',
                artistId     : this.artist.id,
                albumId : this.model.get('id')
            }
        });
    },

    _getImage : function(type) {
        var image = _.where(this.model.get('images'), { coverType : type });

        if (image && image[0]) {
            return image[0].url;
        }

        return undefined;
    },

    _albumSearch : function() {
        CommandController.Execute('albumSearch', {
            name         : 'albumSearch',
            artistId     : this.artist.id,
            albumIds : [this.model.get('id')]
        });
    },

    _albumRename : function() {
        vent.trigger(vent.Commands.ShowRenamePreview, {
            artist       : this.artist,
            albumId : this.model.get('id')
        });
    },

    _albumDetails : function() {
        vent.trigger(vent.Commands.ShowAlbumDetails, { album : this.model });
    },

    _albumMonitored : function() {
        if (!this.artist.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when artist is not monitored',
                type    : 'error'
            });

            return;
        }

        //var savePromise = this.model.save('monitored', !this.model.get('monitored'), { wait : true });
        var savePromise = this.model.save('monitored', !this.model.get('monitored'), { wait : true });

        this.ui.albumMonitored.spinForPromise(savePromise);
    },

    _afterAlbumMonitored : function() {

        this.render();
    },

    _setAlbumMonitoredState : function() {
        this.ui.albumMonitored.removeClass('icon-lidarr-spinner fa-spin');

        if (this.model.get('monitored')) {
            this.ui.albumMonitored.addClass('icon-lidarr-monitored');
            this.ui.albumMonitored.removeClass('icon-lidarr-unmonitored');
        } else {
            this.ui.albumMonitored.addClass('icon-lidarr-unmonitored');
            this.ui.albumMonitored.removeClass('icon-lidarr-monitored');
        }
    },

    _showTracks : function() {
        this.trackGrid.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : this.trackCollection,
            className  : 'table table-hover track-grid'
        }));
    },

    _showAlbumInfo : function() {
        this.albumInfo.show(new AlbumInfoView({
            model                 : this.model
        }));
    },

    _shouldShowTracks : function() {
        var startDate = moment().add('month', -1);
        var endDate = moment().add('year', 1);
        return true;
        //return this.trackCollection.some(function(track) {
        //    var airDate = track.get('releasedDate');

        //    if (airDate) {
        //        var airDateMoment = moment(airDate);

        //        if (airDateMoment.isAfter(startDate) && airDateMoment.isBefore(endDate)) {
        //            return true;
        //        }
        //    }

        //    return false;
        //});
    },

    _showHideTracks : function() {
        if (this.showingTracks) {
            this.showingTracks = false;
            this.trackGrid.close();
        } else {
            this.showingTracks = true;
            this._showTracks();
        }

        this.templateHelpers.showingTracks = this.showingTracks;
        this.render();
    },

    _trackMonitoredToggled : function(options) {
        var model = options.model;
        var shiftKey = options.shiftKey;

        if (!this.trackCollection.get(model.get('id'))) {
            return;
        }

        if (!shiftKey) {
            return;
        }

        var lastToggled = this.trackCollection.lastToggled;

        if (!lastToggled) {
            return;
        }

        var currentIndex = this.trackCollection.indexOf(model);
        var lastIndex = this.trackCollection.indexOf(lastToggled);

        var low = Math.min(currentIndex, lastIndex);
        var high = Math.max(currentIndex, lastIndex);
        var range = _.range(low + 1, high);

        this.trackCollection.lastToggled = model;
    },

    _updateTrackCollection : function() {
        var self = this;

        this.trackCollection.add(this.fullTrackCollection.byAlbum(this.model.get('albumId')).models, { merge : true });

        this.trackCollection.each(function(model) {
            model.trackCollection = self.trackCollection;
        });
    },

    _updateImages : function () {
        var cover = this._getImage('cover');

        if (cover) {
            this.ui.poster.attr('src', cover);
        }
    },

    _refreshTracks : function() {
        this._updateTrackCollection();
        this.trackCollection.fullCollection.sort();
        this.render();
    },

    _openTrackFileEditor : function() {
        //var view = new TrackFileEditorLayout({
        //    model             : this.model,
        //    artist            : this.artist,
        //    trackCollection : this.trackCollection
        //});

        //vent.trigger(vent.Commands.OpenModalCommand, view);
    }
});