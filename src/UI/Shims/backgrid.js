require('backbone');

var backgrid = require('../JsLibraries/backbone.backgrid');
var header = require('../Shared/Grid/HeaderCell');

header.call(backgrid);

backgrid.Column.prototype.defaults = {
    name       : undefined,
    label      : undefined,
    sortable   : true,
    editable   : false,
    renderable : true,
    formatter  : undefined,
    cell       : undefined,
    headerCell : 'Lidarr',
    sortType   : 'toggle'
};
module.exports = backgrid;