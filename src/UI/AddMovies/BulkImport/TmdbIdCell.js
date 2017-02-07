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

            this.$el.html('<input type="text" class="x-tmdbId tmdbId-input form-control" value="' + this.cellValue.get('tmdbId') + '" />');
            
            return this;
        },

        _updateId : function() {
            var data = this.$el.find('.x-tmdbId').val();

            var promise = $.ajax({
                url  : window.NzbDrone.ApiRoot + '/movies/lookup/tmdb?tmdbId=' + data,
                type : 'GET',
            });

            //this.$(this.ui.grab).spinForPromise(promise);
            var _self = this;
            var cacheMonitored = this.model.get('monitored');
            promise.success(function(response) {            
                _self.model.set(response);
                _self.model.set('monitored', cacheMonitored); //reset to the previous monitored value
            });

            promise.error(function(request, status, error) {
                console.error("Status: " + status, "Error: " + error);
            });
        }
});
