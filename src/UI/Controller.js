var NzbDroneController = require('./Shared/NzbDroneController');
var AppLayout = require('./AppLayout');
var Marionette = require('marionette');
var ActivityLayout = require('./Activity/ActivityLayout');
var SettingsLayout = require('./Settings/SettingsLayout');
//var AddSeriesLayout = require('./AddSeries/AddSeriesLayout');
var AddArtistLayout = require('./AddArtist/AddArtistLayout');
var WantedLayout = require('./Wanted/WantedLayout');
var CalendarLayout = require('./Calendar/CalendarLayout');
var ReleaseLayout = require('./Release/ReleaseLayout');
var SystemLayout = require('./System/SystemLayout');
var AlbumStudioLayout = require('./AlbumStudio/AlbumStudioLayout');
//var SeriesEditorLayout = require('./Series/Editor/SeriesEditorLayout');
var ArtistEditorLayout = require('./Artist/Editor/ArtistEditorLayout');

module.exports = NzbDroneController.extend({
    addArtist : function(action) {
        this.setTitle('Add Artist');
        this.showMainRegion(new AddArtistLayout({ action : action }));
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

    albumStudio : function() {
        this.setTitle('Album Studio');
        this.showMainRegion(new AlbumStudioLayout());
    },

    artistEditor : function() {
        this.setTitle('Artist Editor');
        this.showMainRegion(new ArtistEditorLayout());
    }
});