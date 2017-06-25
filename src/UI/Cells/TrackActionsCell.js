var vent = require('vent');
var NzbDroneCell = require('./NzbDroneCell');
var CommandController = require('../Commands/CommandController');

module.exports = NzbDroneCell.extend({
    className : 'track-actions-cell',

    events : {
        'click .x-automatic-search' : '_automaticSearch',
        'click .x-manual-search'    : '_manualSearch'
    },

    render : function() {
        this.$el.empty();

        this.$el.html('<i class="icon-lidarr-search x-automatic-search" title="Automatic Search"></i>' + '<i class="icon-lidarr-search-manual x-manual-search" title="Manual Search"></i>');

        CommandController.bindToCommand({
            element : this.$el.find('.x-automatic-search'),
            command : {
                name       : 'trackSearch',
                trackIds : [this.model.get('id')]
            }
        });

        this.delegateEvents();
        return this;
    },

    _automaticSearch : function() {
        CommandController.Execute('trackSearch', {
            name       : 'trackSearch',
            trackIds : [this.model.get('id')]
        });
    },

    _manualSearch : function() {
        vent.trigger(vent.Commands.ShowTrackDetails, {
            track        : this.cellValue,
            hideSeriesLink : true,
            openingTab     : 'search'
        });
    }
});