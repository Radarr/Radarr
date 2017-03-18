var NzbDroneController = require('./Shared/NzbDroneController');
var AppLayout = require('./AppLayout');
var Marionette = require('marionette');
var ActivityLayout = require('./Activity/ActivityLayout');
var SettingsLayout = require('./Settings/SettingsLayout');
var AddSeriesLayout = require('./AddSeries/AddSeriesLayout');
var AddMoviesLayout = require('./AddMovies/AddMoviesLayout');
var WantedLayout = require('./Wanted/WantedLayout');
var CalendarLayout = require('./Calendar/CalendarLayout');
var ReleaseLayout = require('./Release/ReleaseLayout');
var SystemLayout = require('./System/SystemLayout');
var SeasonPassLayout = require('./SeasonPass/SeasonPassLayout');
var SeriesEditorLayout = require('./Series/Editor/SeriesEditorLayout');
var MovieEditorLayout = require('./Movies/Editor/MovieEditorLayout');

module.exports = NzbDroneController.extend({
    addSeries : function(action) {
        this.setTitle('Add Movie');
        this.showMainRegion(new AddSeriesLayout({ action : action }));
    },

    addMovies : function(action, query) {
      this.setTitle("Add Movie");
      this.showMainRegion(new AddMoviesLayout({ action : action, query : query }));
    },

    calendar : function() {
        this.setTitle('Calendar');
        this.showMainRegion(new CalendarLayout());
    },

    settings : function(action) {
        this.setTitle('Settings');
        this.showMainRegion(new SettingsLayout({ action : action }));
    },

    wanted : function(action) {
        this.setTitle('Wanted');
        this.showMainRegion(new WantedLayout({ action : action }));
    },

    activity : function(action) {
        this.setTitle('Activity');
        this.showMainRegion(new ActivityLayout({ action : action }));
    },

    rss : function() {
        this.setTitle('RSS');
        this.showMainRegion(new ReleaseLayout());
    },

    system : function(action) {
        this.setTitle('System');
        this.showMainRegion(new SystemLayout({ action : action }));
    },

    seasonPass : function() {
        this.setTitle('Season Pass');
        this.showMainRegion(new SeasonPassLayout());
    },

    seriesEditor : function() {
        this.setTitle('Series Editor');
        this.showMainRegion(new SeriesEditorLayout());
    },

    movieEditor : function() {
        this.setTitle('Movie Editor');
        this.showMainRegion(new MovieEditorLayout());
    }
});
