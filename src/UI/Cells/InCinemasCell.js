var TemplatedCell = require('./TemplatedCell');
var moment = require('moment');
var FormatHelpers = require('../Shared/FormatHelpers');
var UiSettingsModel = require('../Shared/UiSettingsModel');

module.exports = TemplatedCell.extend({
    className : 'in-cinemas-cell',

    render : function() {
      this.$el.html("");

      if (this.model.get("inCinemas")) {
        var cinemasDate = this.model.get("inCinemas");
        this.$el.html(moment(cinemasDate).format(UiSettingsModel.shortDate()));
      }

      return this;
    }
});
