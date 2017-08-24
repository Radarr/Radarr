var vent = require('vent');
var _ = require('underscore');
var $ = require('jquery');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var CommandController = require('../../Commands/CommandController');

module.exports = NzbDroneCell.extend({
        className : 'foreignId-cell',

        events : {
            'blur input.foreignId-input' : '_updateId'
        },

        render : function() {
            this.$el.empty();

            this.$el.html('<i class="icon-lidarr-info hidden"></i><input type="text" class="x-foreignId foreignId-input form-control" value="' + this.cellValue.get('foreignArtistId') + '" />');

            return this;
        },

        _updateId : function() {
            var field = this.$el.find('.x-foreignId');
            var data = field.val();

            var promise = $.ajax({
                url  : window.NzbDrone.ApiRoot + '/artist/lookup?term=lidarrid:' + data,
                type : 'GET',
            });

            field.prop('disabled', true);

            var icon = this.$('.icon-lidarr-info');

            icon.removeClass('hidden');

            icon.spinForPromise(promise);
            var _self = this;
            var cacheMonitored = this.model.get('monitored');
            var cacheProfile = this.model.get('profileId');
            var cachePath = this.model.get('path');
            var cacheRoot = this.model.get('rootFolderPath');

            promise.success(function(response) {
                _self.model.set(response[0]);
                _self.model.set('monitored', cacheMonitored);
                _self.model.set('profileId', cacheProfile);
                _self.model.set('path', cachePath);
                field.prop('disabled', false);
            });

            promise.error(function(request, status, error) {
                console.error('Status: ' + status, 'Error: ' + error);
                field.prop('disabled', false);
            });
        }
});
