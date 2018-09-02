var Backgrid = require('backgrid');
var Marionette = require('marionette');
require('bootstrap');

module.exports = Backgrid.Cell.extend({
    className : 'edition-cell',
    //template  : 'Cells/EditionCellTemplate',

    render : function() {

        var edition = this.model.get(this.column.get('name'));
        if (!edition) {
          return this;
        }
        var cut = false;

        if (edition.toLowerCase().contains("cut")) {
          cut = true;
        }

        //this.templateFunction = Marionette.TemplateCache.get(this.template);

        //var html = this.templateFunction(edition);
        if (cut) {
          this.$el.html('<i class="icon-radarr-form-cut"/ title="{0}">'.format(edition));
        } else {
          this.$el.html('<i class="icon-radarr-form-special"/ title="{0}">'.format(edition));
        }

        /*this.$el.popover({
            content   : html,
            html      : true,
            trigger   : 'hover',
            title     : this.column.get('title'),
            placement : 'left',
            container : this.$el
        });*/

        return this;
    }
});
