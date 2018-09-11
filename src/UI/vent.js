var Wreqr = require('./JsLibraries/backbone.wreqr');

var vent = new Wreqr.EventAggregator();

vent.Events = {
		MovieAdded         : 'movie:added',
		MovieDeleted       : 'movie:deleted',
		CommandComplete    : 'command:complete',
		ServerUpdated      : 'server:updated',
		EpisodeFileDeleted : 'episodefile:deleted',
		CustomFormatsChanged : 'customformat:changed'
};

vent.Commands = {
		EditMovieCommand         : 'EditMovieCommand',
		EditFileCommand          : "EditFileCommand",
		DeleteMovieCommand       : 'DeleteMovieCommand',
		OpenModalCommand         : 'OpenModalCommand',
		CloseModalCommand        : 'CloseModalCommand',
		OpenModal2Command        : 'OpenModal2Command',
		CloseModal2Command       : 'CloseModal2Command',
		ShowMovieDetails         : 'ShowMovieDetails',
		ShowHistoryDetails       : 'ShowHistoryDetails',
		ShowLogDetails           : 'ShowLogDetails',
		SaveSettings             : 'saveSettings',
		ShowLogFile              : 'showLogFile',
		ShowRenamePreview        : 'showRenamePreview',
		ShowManualImport         : 'showManualImport',
		ShowFileBrowser          : 'showFileBrowser',
		CloseFileBrowser         : 'closeFileBrowser',
		OpenControlPanelCommand  : 'OpenControlPanelCommand',
		CloseControlPanelCommand : 'CloseControlPanelCommand',
		ShowExistingCommand      : 'ShowExistingCommand',
		MovieFileEdited			 : 'MovieFileEdited'
};

vent.Hotkeys = {
		NavbarSearch : 'navbar:search',
		SaveSettings : 'settings:save',
		ShowHotkeys  : 'hotkeys:show'
};

module.exports = vent;
