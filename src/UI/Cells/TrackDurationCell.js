var Backgrid = require('backgrid');
var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = Backgrid.Cell.extend({
    className : 'track-duration-cell',

    render : function() {
        var duration = this.model.get(this.column.get('name'));
        this.$el.html(FormatHelpers.timeMinSec(duration,'ms'));
        this.delegateEvents();
        return this;
    }
});