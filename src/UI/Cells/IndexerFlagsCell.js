var Backgrid = require('backgrid');
var Marionette = require('marionette');
require('bootstrap');

module.exports = Backgrid.Cell.extend({
    className : 'edition-cell',
    //template  : 'Cells/EditionCellTemplate',

    render : function() {

        var flags = this.model.get("indexerFlags");
        if (!flags) {
          return this;
        }

        var html = "";

        if (flags) {
          _.each(flags, function(flag){
            var addon = "";
            var title = "";

            switch (flag) {
              case "G_Freeleech":
              addon = "⬇";
              title = "Freeleech";
              break;
              case "G_Halfleech":
              addon = "⇩";
              title = "50% Freeleech";
              break;
              case "G_DoubleUpload":
              addon = "⬆";
              title = "Double upload";
              break;
              case "PTP_Golden":
              addon = "🍿";
              title = "Golden";
              break;
              case "PTP_Approved":
              addon = "✔";
              title = "Approved by PTP"
              break;
              case "HDB_Internal":
              addon = "⭐️";
              title = "HDBits Internal";
              break;
            }
            if (addon != "") {
              html += "<span title='{0}'>{1}</span> ".format(title, addon);
            }
          });
        }

        this.$el.html(html);

        return this;
    }
});
