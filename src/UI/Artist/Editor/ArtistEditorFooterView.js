var _ = require('underscore');
var Marionette = require('marionette');
var vent = require('vent');
var Profiles = require('../../Profile/ProfileCollection');
var RootFolders = require('../../AddArtist/RootFolders/RootFolderCollection');
var RootFolderLayout = require('../../AddArtist/RootFolders/RootFolderLayout');
var UpdateFilesArtistView = require('./Organize/OrganizeFilesView');
var Config = require('../../Config');

module.exports = Marionette.ItemView.extend({
    template : 'Artist/Editor/ArtistEditorFooterViewTemplate',

    ui : {
        monitored           : '.x-monitored',
        profile             : '.x-profiles',
        albumFolder         : '.x-album-folder',
        rootFolder          : '.x-root-folder',
        selectedCount       : '.x-selected-count',
        container           : '.artist-editor-footer',
        actions             : '.x-action'
    },

    events : {
        'click .x-save'           : '_updateAndSave',
        'change .x-root-folder'   : '_rootFolderChanged',
        'click .x-organize-files' : '_organizeFiles'
    },

    templateHelpers : function() {
        return {
            profiles    : Profiles,
            rootFolders : RootFolders.toJSON()
        };
    },

    initialize : function(options) {
        this.artistCollection = options.collection;

        RootFolders.fetch().done(function() {
            RootFolders.synced = true;
        });

        this.editorGrid = options.editorGrid;
        this.listenTo(this.artistCollection, 'backgrid:selected', this._updateInfo);
        this.listenTo(RootFolders, 'all', this.render);
    },

    onRender : function() {
        this._updateInfo();
    },

    _updateAndSave : function() {
        var selected = this.editorGrid.getSelectedModels();

        var monitored = this.ui.monitored.val();
        var profile = this.ui.profile.val();
        var albumFolder = this.ui.albumFolder.val();
        var rootFolder = this.ui.rootFolder.val();

        _.each(selected, function(model) {
            if (monitored === 'true') {
                model.set('monitored', true);
            } else if (monitored === 'false') {
                model.set('monitored', false);
            }

            if (profile !== 'noChange') {
                model.set('profileId', parseInt(profile, 10));
            }

            if (albumFolder === 'true') {
                model.set('albumFolder', true);
            } else if (albumFolder === 'false') {
                model.set('albumFolder', false);
            }

            if (rootFolder !== 'noChange') {
                var rootFolderPath = RootFolders.get(parseInt(rootFolder, 10));

                model.set('rootFolderPath', rootFolderPath.get('path'));
            }

            model.edited = true;
        });

        this.artistCollection.save();
    },

    _updateInfo : function() {
        var selected = this.editorGrid.getSelectedModels();
        var selectedCount = selected.length;

        this.ui.selectedCount.html('{0} artist selected'.format(selectedCount));

        if (selectedCount === 0) {
            this.ui.actions.attr('disabled', 'disabled');
        } else {
            this.ui.actions.removeAttr('disabled');
        }
    },

    _rootFolderChanged : function() {
        var rootFolderValue = this.ui.rootFolder.val();
        if (rootFolderValue === 'addNew') {
            var rootFolderLayout = new RootFolderLayout();
            this.listenToOnce(rootFolderLayout, 'folderSelected', this._setRootFolder);
            vent.trigger(vent.Commands.OpenModalCommand, rootFolderLayout);
        } else {
            Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
        }
    },

    _setRootFolder : function(options) {
        vent.trigger(vent.Commands.CloseModalCommand);
        this.ui.rootFolder.val(options.model.id);
        this._rootFolderChanged();
    },

    _organizeFiles : function() {
        var selected = this.editorGrid.getSelectedModels();
        var updateFilesArtistView = new UpdateFilesArtistView({ artist : selected });
        this.listenToOnce(updateFilesArtistView, 'updatingFiles', this._afterSave);

        vent.trigger(vent.Commands.OpenModalCommand, updateFilesSeriesView);
    }
});