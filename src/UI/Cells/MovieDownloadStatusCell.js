var TemplatedCell = require('./TemplatedCell');

module.exports = TemplatedCell.extend({
    className : 'movie-title-cell',
    template  : 'Cells/MovieDownloadStatusTemplate',
    sortKey : function(model) {
      debugger;
      return 0;
    }
});
