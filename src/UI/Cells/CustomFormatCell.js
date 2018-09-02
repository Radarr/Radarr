var TemplatedCell = require('./TemplatedCell');
var _ = require('underscore');

module.exports = TemplatedCell.extend({
    className : 'matches-cell',
    template  : 'Cells/CustomFormatCell',
    _orig     : TemplatedCell.prototype.initialize,

    initialize : function() {
        this._orig.apply(this, arguments);
    }

});
