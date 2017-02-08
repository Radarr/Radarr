var TemplatedCell = require('../../Cells/TemplatedCell');
var QualityCellEditor = require('../../Cells/Edit/QualityCellEditor');

module.exports = TemplatedCell.extend({
		className : 'quality-cell',
		template  : 'AddMovies/BulkImport/QualityCellTemplate',
		editor    : QualityCellEditor
});
