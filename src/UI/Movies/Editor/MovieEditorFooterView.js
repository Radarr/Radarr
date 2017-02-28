var _ = require('underscore');
var Marionette = require('marionette');
var vent = require('vent');
var Profiles = require('../../Profile/ProfileCollection');
var RootFolders = require('../../AddMovies/RootFolders/RootFolderCollection');
var RootFolderLayout = require('../../AddMovies/RootFolders/RootFolderLayout');
var UpdateFilesMoviesView = require('./Organize/OrganizeFilesView');
var Config = require('../../Config');
//var FullMovieCollection = require('../FullMovieCollection');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Editor/MovieEditorFooterViewTemplate',

    ui : {
        monitored           : '.x-monitored',
        profile             : '.x-profiles',
	minimumAvailability : '.x-minimumavailability',
        seasonFolder        : '.x-season-folder',
        rootFolder          : '.x-root-folder',
        selectedCount       : '.x-selected-count',
        container           : '.series-editor-footer',
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
        this.moviesCollection = options.collection;
		this.fullMovieCollection = options.c2;

        RootFolders.fetch().done(function() {
            RootFolders.synced = true;
        });

        this.editorGrid = options.editorGrid;
        //this.listenTo(this.moviesCollection, 'backgrid:selected', this._updateInfo);


        this.listenTo(this.moviesCollection, 'backgrid:selected', function(model, selected) {
            //window.alert(selected);
            var m =  this.fullMovieCollection.findWhere({ tmdbId : parseInt(model.get('tmdbId'),10) });
			//window.alert('we are here');
			//window.alert(selected);
            m.set('selected', selected);
            /*if (selected) {
                this.selectedCount++;
            } else {
                this.selectedCount--;
            }*/
            this._updateInfo();
        });



        this.listenTo(RootFolders, 'all', this.render);
    },

    onRender : function() {
        this._updateInfo();
    },

    _updateAndSave : function() {
        //var selected = this.editorGrid.getSelectedModels();

		var selected = this.fullMovieCollection.where({ selected : true });
        var monitored = this.ui.monitored.val();
		var minAvail = this.ui.minimumAvailability.val();
        var profile = this.ui.profile.val();
        var seasonFolder = this.ui.seasonFolder.val();
        var rootFolder = this.ui.rootFolder.val();

		


        _.each(selected, function(model) {
  			//window.alert(' we are here');
			//window.alert('reseting selection');
			//var m = this.moviesCollection.findWhere({ tmdbId : parseInt(model.get('tmdbId'),10) });
			//m.trigger('backgrid:select', model, false);
            //window.alert('done triggering');


            if (monitored === 'true') {
                model.set('monitored', true);
            } else if (monitored === 'false') {
                model.set('monitored', false);
            }

            if (minAvail !=='noChange') {
				model.set('minimumAvailability', minAvail);
	    	}

            if (profile !== 'noChange') {
                model.set('profileId', parseInt(profile, 10));
            }

            if (seasonFolder === 'true') {
                model.set('seasonFolder', true);
            } else if (seasonFolder === 'false') {
                model.set('seasonFolder', false);
            }

            if (rootFolder !== 'noChange') {
                var rootFolderPath = RootFolders.get(parseInt(rootFolder, 10));

                model.set('rootFolderPath', rootFolderPath.get('path'));
            }
            model.edited = true;
        });
 		//window.alert('about to save');
        this.fullMovieCollection.save();
		this.moviesCollection.save();
		//window.alert('done saving');
    },

    _updateInfo : function() {
        var selected = this.editorGrid.getSelectedModels();
        var selectedCount = selected.length;

        this.ui.selectedCount.html('{0} movies selected'.format(selectedCount));

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
        var updateFilesMoviesView = new UpdateFilesMoviesView({ movies : selected });
        this.listenToOnce(updateFilesMoviesView, 'updatingFiles', this._afterSave);

        vent.trigger(vent.Commands.OpenModalCommand, updateFilesMoviesView);
    }
});
