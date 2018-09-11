var _ = require('underscore');
var Marionette = require('marionette');
var vent = require('vent');
var Profiles = require('../../Profile/ProfileCollection');
var RootFolders = require('../../AddMovies/RootFolders/RootFolderCollection');
var RootFolderLayout = require('../../AddMovies/RootFolders/RootFolderLayout');
var UpdateFilesMoviesView = require('./Organize/OrganizeFilesView');
var Config = require('../../Config');
var FullMovieCollection = require('../FullMovieCollection');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Editor/MovieEditorFooterViewTemplate',

    ui : {
        monitored           : '.x-monitored',
        profile             : '.x-profiles',
	minimumAvailability : '.x-minimumavailability',
        staticPath        : '.x-static-path',
        rootFolder          : '.x-root-folder',
        selectedCount       : '.x-selected-count',
        container           : '.movie-editor-footer',
        actions             : '.x-action'
    },

    events : {
        'click .x-save'           : '_updateAndSave',
        'change .x-root-folder'   : '_rootFolderChanged',
        'click .x-organize-files' : '_organizeFiles',
        'click .x-update-quality' : '_updateQuality'
    },

    templateHelpers : function() {
        return {
            profiles    : Profiles,
            rootFolders : RootFolders.toJSON()
        };
    },

    initialize : function(options) {
        this.moviesCollection = options.collection;
        RootFolders.fetch().done(function() {
            RootFolders.synced = true;
        });

        this.editorGrid = options.editorGrid;


        this.listenTo(this.moviesCollection, 'backgrid:selected', function(model, selected) {
            var m =  FullMovieCollection.findWhere({ tmdbId : model.get('tmdbId') });
            m.set('selected', selected);
            this._updateInfo();
        });

        this.listenTo(FullMovieCollection, 'save', function() {
			window.alert(' Done Saving');
			var selected = FullMovieCollection.where({ selected : true });
		});


        this.listenTo(RootFolders, 'all', this.render);
    },

    onRender : function() {
        this._updateInfo();
    },

    _updateAndSave : function() {
        //var selected = this.editorGrid.getSelectedModels();

		var selected = FullMovieCollection.where({ selected : true });
        var monitored = this.ui.monitored.val();
		var minAvail = this.ui.minimumAvailability.val();
        var profile = this.ui.profile.val();
        var staticPath = this.ui.staticPath.val();
        var rootFolder = this.ui.rootFolder.val();

		var i = 0;
		var b = [];
        _.each(selected, function(model) {

            b[i] = model.get('tmdbId');
						i++;
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

            if (staticPath !== 'noChange') {
                model.set('pathState', staticPath);
            }

            if (rootFolder !== 'noChange') {
                var rootFolderPath = RootFolders.get(parseInt(rootFolder, 10));

                model.set('rootFolderPath', rootFolderPath.get('path'));
            }
            model.edited = true;
        });
        var filterKey = this.moviesCollection.state.filterKey;
        var filterValue = this.moviesCollection.state.filterValue;
		var currentPage = this.moviesCollection.state.currentPage;
        this.moviesCollection.setFilterMode('all');
		//this.moviesCollection.fullCollection.resetFiltered();
		for (var j=0; j<i; j++) {
				var m = this.moviesCollection.fullCollection.findWhere({ tmdbId : b[j] });
				if (m!== undefined) {
      			if (monitored === 'true') {
          			m.set('monitored', true);
                } else if (monitored === 'false') {
                    m.set('monitored', false);
                }

                if (minAvail !=='noChange') {
                    m.set('minimumAvailability', minAvail);
                }

                if (profile !== 'noChange') {
                    m.set('profileId', parseInt(profile, 10));
                }

                if (staticPath !== 'noChange') {
                    m.set('pathState', staticPath);
                }

                if (rootFolder !== 'noChange') {
                	var rootFolderPath = RootFolders.get(parseInt(rootFolder, 10));
					var folderName = m.get('folderName');
                	//m.set('path', rootFolderPath.get('path')+ folderName);
            	}
			}
		}
		this.moviesCollection.state.filterKey = filterKey;
        this.moviesCollection.state.filterValue = filterValue;
        this.moviesCollection.fullCollection.resetFiltered();
		this.moviesCollection.getPage(currentPage, { fetch: false});

		FullMovieCollection.save();
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
        var selected = FullMovieCollection.where({ selected : true });
        var updateFilesMoviesView = new UpdateFilesMoviesView({ movies : selected });
        this.listenToOnce(updateFilesMoviesView, 'updatingFiles', this._afterSave);

        vent.trigger(vent.Commands.OpenModalCommand, updateFilesMoviesView);
    }
});
