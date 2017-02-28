var TemplatedCell = require('./TemplatedCell');
var moment = require('moment');

module.exports = TemplatedCell.extend({
    className : 'date-added-cell',

    render : function() {
      var monthNames = ["January", "February", "March", "April", "May", "June",
      "July", "August", "September", "October", "November", "December"
    ];

      this.$el.html("");

      if (this.model.get("added")) {
          this.$el.html(moment(this.model.get("added")).format('LLL'));
      }

      return this;
    }
});
