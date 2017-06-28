var vent = require('vent');
var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditArtistView = require('../../Artist/Edit/EditArtistView');
var DeleteArtistView = require('../../Artist/Delete/DeleteArtistView');
var EpisodeDetailsLayout = require('../../Episode/EpisodeDetailsLayout');
var AlbumDetailsLayout = require('../../Album/AlbumDetailsLayout');
var HistoryDetailsLayout = require('../../Activity/History/Details/HistoryDetailsLayout');
var LogDetailsView = require('../../System/Logs/Table/Details/LogDetailsView');
var RenamePreviewLayout = require('../../Rename/RenamePreviewLayout');
var ManualImportLayout = require('../../ManualImport/ManualImportLayout');
var FileBrowserLayout = require('../FileBrowser/FileBrowserLayout');

module.exports = Marionette.AppRouter.extend({
    initialize : function() {
        vent.on(vent.Commands.OpenModalCommand, this._openModal, this);
        vent.on(vent.Commands.CloseModalCommand, this._closeModal, this);
        vent.on(vent.Commands.OpenModal2Command, this._openModal2, this);
        vent.on(vent.Commands.CloseModal2Command, this._closeModal2, this);
        vent.on(vent.Commands.EditArtistCommand, this._editArtist, this);
        vent.on(vent.Commands.DeleteArtistCommand, this._deleteArtist, this);
        vent.on(vent.Commands.ShowEpisodeDetails, this._showEpisode, this);
        vent.on(vent.Commands.ShowAlbumDetails, this._showAlbum, this);
        vent.on(vent.Commands.ShowHistoryDetails, this._showHistory, this);
        vent.on(vent.Commands.ShowLogDetails, this._showLogDetails, this);
        vent.on(vent.Commands.ShowRenamePreview, this._showRenamePreview, this);
        vent.on(vent.Commands.ShowManualImport, this._showManualImport, this);
        vent.on(vent.Commands.ShowFileBrowser, this._showFileBrowser, this);
        vent.on(vent.Commands.CloseFileBrowser, this._closeFileBrowser, this);
    },

    _openModal : function(view) {
        AppLayout.modalRegion.show(view);
    },

    _closeModal : function() {
        AppLayout.modalRegion.closeModal();
    },

    _openModal2 : function(view) {
        AppLayout.modalRegion2.show(view);
    },

    _closeModal2 : function() {
        AppLayout.modalRegion2.closeModal();
    },

    _editArtist : function(options) {
        var view = new EditArtistView({ model : options.artist });
        AppLayout.modalRegion.show(view);
    },

    _deleteArtist : function(options) {
        var view = new DeleteArtistView({ model : options.artist });
        AppLayout.modalRegion.show(view);
    },

    _showEpisode : function(options) {
        var view = new EpisodeDetailsLayout({
            model          : options.episode,
            hideSeriesLink : options.hideSeriesLink,
            openingTab     : options.openingTab
        });
        AppLayout.modalRegion.show(view);
    },

    _showAlbum : function(options) {
        var view = new AlbumDetailsLayout({
            model          : options.album
        });
        AppLayout.modalRegion.show(view);
    },

    _showHistory : function(options) {
        var view = new HistoryDetailsLayout({ model : options.model });
        AppLayout.modalRegion.show(view);
    },

    _showLogDetails : function(options) {
        var view = new LogDetailsView({ model : options.model });
        AppLayout.modalRegion.show(view);
    },

    _showRenamePreview : function(options) {
        var view = new RenamePreviewLayout(options);
        AppLayout.modalRegion.show(view);
    },

    _showManualImport : function(options) {
        var view = new ManualImportLayout(options);
        AppLayout.modalRegion.show(view);
    },

    _showFileBrowser : function(options) {
        var view = new FileBrowserLayout(options);
        AppLayout.modalRegion2.show(view);
    },

    _closeFileBrowser : function() {
        AppLayout.modalRegion2.closeModal();
    }
});