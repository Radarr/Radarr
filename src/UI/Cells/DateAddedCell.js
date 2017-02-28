var TemplatedCell = require('./TemplatedCell');
var UiSettingsModel = require('../Shared/UiSettingsModel');
var FormatHelpers = require('../Shared/FormatHelpers');
var moment = require('moment');

module.exports = TemplatedCell.extend({
    className : 'date-added-cell',

    render : function() {
      this.$el.html("");

      var dateAdded = this.model.get("added");

      if (dateAdded) {
          var time = '{0} at {1}'.format(FormatHelpers.relativeDate(dateAdded), moment(dateAdded).format(UiSettingsModel.time(true, false)));
          this.$el.html(time);
      }

      return this;
    }
});
