var Backgrid = require('backgrid');
var Marionette = require('marionette');
require('bootstrap');

module.exports = Backgrid.Cell.extend({
    className : 'edition-cell',
    //template  : 'Cells/EditionCellTemplate',

    render : function() {

        var edition = this.model.get("indexerFlags");
        if (!edition) {
          return this;
        }

        var html = "";

        if (flags) {
          _.each(flags, function(flag){
            var addon = "";
            var title = "";

            switch (flag) {
              case "G_Freeleech":
              addon = "";
              break;
              case "G_Halfleech":
              addon = "";
              break;
              case "G_DoubleUpload":
              addon = "";
              break;
              case "PTP_Golden":
              addon = "🍿";
              break;
              case "PTP_Approved":
              addon = "✔";
              break;
            }
            if (addon != "") {
              html += "<span title={0}>{1}</span>".format(title, addon);
            }
          });
        }

        return this;
    }
});
