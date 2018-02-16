var _ = require('underscore');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template  : 'ManualImport/Summary/ManualImportSummaryViewTemplate',

    initialize : function (options) {

        this.templateHelpers = {
            file     : options.file,
            movie    : options.movie,
            quality  : options.quality
        };
    }
});