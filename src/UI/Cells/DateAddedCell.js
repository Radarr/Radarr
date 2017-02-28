var TemplatedCell = require('./TemplatedCell');
var moment = require('moment');

module.exports = TemplatedCell.extend({
    className : 'date-added-cell',

    render : function() {
      this.$el.html("");

      if (this.model.get("added")) {
          this.$el.html(moment(this.model.get("added")).format('LLL'));
      }

      return this;
    }
});
