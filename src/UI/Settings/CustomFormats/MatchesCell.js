var TemplatedCell = require('../../Cells/TemplatedCell');
var _ = require('underscore');
require('./FormatTagHelpers');

module.exports = TemplatedCell.extend({
    className : 'matches-cell',
    template  : 'Settings/CustomFormats/MatchesCell'
});
