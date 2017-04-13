var _ = require('underscore');
var Backgrid = require('backgrid');

var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = Backgrid.Cell.extend({
    className : 'title-cell',

    render : function() {
      debugger;
        var title = this.model.get('title');
        var flags = this.model.get("indexerFlags");
        if (flags) {
          _.each(flags, function(flag){
            var addon = "";
            debugger;
            switch (flag) {
              case "PTP_Golden":
              addon = "🍿";
              break;
              case "PTP_Approved":
              addon = "✔";
              break;
            }

            title += addon;
          });
        }
        this.$el.html(title);

        return this;
    }
});
