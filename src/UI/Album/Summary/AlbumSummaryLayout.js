var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var TrackFileModel = require('../../Artist/TrackFileModel');
var TrackFileCollection = require('../../Artist/TrackFileCollection');
var FileSizeCell = require('../../Cells/FileSizeCell');
var QualityCell = require('../../Cells/QualityCell');
var DeleteEpisodeFileCell = require('../../Cells/DeleteEpisodeFileCell');
var NoFileView = require('./NoFileView');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'Album/Summary/AlbumSummaryLayoutTemplate',

    regions : {
        overview : '.album-overview',
        activity : '.album-file-info'
    },

    columns : [
        {
            name     : 'path',
            label    : 'Path',
            cell     : 'string',
            sortable : false
        },
        {
            name     : 'size',
            label    : 'Size',
            cell     : FileSizeCell,
            sortable : false
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
            sortable : false,
            editable : true
        },
        {
            name     : 'this',
            label    : '',
            cell     : DeleteEpisodeFileCell,
            sortable : false
        }
    ],

    templateHelpers : {},

    initialize : function(options) {
        if (!this.model.artist) {
            this.templateHelpers.artist = options.artist.toJSON();
        }
    },

    onShow : function() {
        if (this.model.get('hasFile')) { //TODO Refactor for Albums
            var episodeFileId = this.model.get('episodeFileId');

            if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, episodeFileId);
                this.trackFileCollection = new TrackFileCollection(episodeFile, { seriesId : this.model.get('seriesId') });
                this.listenTo(episodeFile, 'destroy', this._episodeFileDeleted);

                this._showTable();
            }

            else {
                this.activity.show(new LoadingView());

                var self = this;
                var newEpisodeFile = new TrackFileModel({ id : episodeFileId });
                this.episodeFileCollection = new TrackFileCollection(newEpisodeFile, { seriesId : this.model.get('seriesId') });
                var promise = newEpisodeFile.fetch();
                this.listenTo(newEpisodeFile, 'destroy', this._trackFileDeleted);

                promise.done(function() {
                    self._showTable();
                });
            }

            this.listenTo(this.episodeFileCollection, 'add remove', this._collectionChanged);
        }

        else {
            this._showNoFileView();
        }
    },

    _showTable : function() {
        this.activity.show(new Backgrid.Grid({
            collection : this.trackFileCollection,
            columns    : this.columns,
            className  : 'table table-bordered',
            emptyText  : 'Nothing to see here!'
        }));
    },

    _showNoFileView : function() {
        this.activity.show(new NoFileView());
    },

    _collectionChanged : function() {
        if (!this.trackFileCollection.any()) {
            this._showNoFileView();
        }

        else {
            this._showTable();
        }
    },

    _trackFileDeleted : function() {
        this.model.set({
            trackFileId   : 0,
            hasFile       : false
        });
    }
});