var vent = require('vent');
var _ = require('underscore');
var $ = require('jquery');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var CommandController = require('../../Commands/CommandController');

module.exports = NzbDroneCell.extend({
		className : 'tmdbId-cell',

        // would like to use change with a _.debounce eventually
        events : {
            'blur input.tmdbId-input' : '_updateId'
        },

        render : function() {
            this.$el.empty();

            this.$el.html('<i class="icon-radarr-info hidden"></i><input type="text" class="x-tmdbId tmdbId-input form-control" value="' + this.cellValue.get('tmdbId') + '" />');

            return this;
        },

        _updateId : function() {
            var field = this.$el.find('.x-tmdbId');
            var data = field.val();

            var promise = $.ajax({
                url  : window.NzbDrone.ApiRoot + '/movie/lookup/tmdb?tmdbId=' + data,
                type : 'GET',
            });

						//field.spinForPromise(promise);

            field.prop("disabled", true);

            var icon = this.$(".icon-radarr-info");

            icon.removeClass("hidden");

            icon.spinForPromise(promise);
            var _self = this;
            var cacheMonitored = this.model.get('monitored');
            var cacheProfile = this.model.get("profileId");
            var cachePath = this.model.get("path");
            var cacheFile = this.model.get("movieFile");
            var cacheRoot = this.model.get("rootFolderPath");

            promise.success(function(response) {
                _self.model.set(response);
                _self.model.set('monitored', cacheMonitored); //reset to the previous monitored value
                _self.model.set('profileId', cacheProfile);
                _self.model.set('path', cachePath);
                _self.model.set('movieFile', cacheFile); // may be unneccessary.
                field.prop("disabled", false);
            });

            promise.error(function(request, status, error) {
                console.error("Status: " + status, "Error: " + error);
                field.prop("disabled", false);
            });
        }
});
