var vent = require('vent');
var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'delete-episode-file-cell',

    events : {
        'click' : '_onClick'
    },

    render : function() {
        this.$el.empty();
        this.$el.html('<i class="icon-radarr-delete" title="Delete exclusion."></i>');

        return this;
    },

    _onClick : function() {
        var self = this;

            this.model.destroy();

    }
});
