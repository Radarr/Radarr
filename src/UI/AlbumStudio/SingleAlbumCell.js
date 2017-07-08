var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ToggleCell = require('../Cells/TrackMonitoredCell');
var CommandController = require('../Commands/CommandController');
var moment = require('moment');
var _ = require('underscore');
var Messenger = require('../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    template : 'AlbumStudio/SingleAlbumCellTemplate',

    ui : {
        albumMonitored : '.x-album-monitored'
    },

    events : {
        'click .x-album-monitored'           : '_albumMonitored'
    },


    initialize : function(options) {
        this.artist = options.artist;
        this.listenTo(this.model, 'sync', this._afterAlbumMonitored);

    },

    onRender : function() {
        this._setAlbumMonitoredState();
    },

    _albumMonitored : function() {
        if (!this.artist.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when artist is not monitored',
                type    : 'error'
            });

            return;
        }

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
     }

});