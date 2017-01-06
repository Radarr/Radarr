var TemplatedCell = require('./TemplatedCell');

module.exports = TemplatedCell.extend({
    className : 'in-cinemas-cell',

    render : function() {
      var monthNames = ["January", "February", "March", "April", "May", "June",
      "July", "August", "September", "October", "November", "December"
    ];
      var cinemasDate = new Date(this.model.get("inCinemas"));
      var year = cinemasDate.getFullYear();
      var month = monthNames[cinemasDate.getMonth()];
        this.$el.html(month + " " + year); //Hack, but somehow handlebar helper does not work.
        return this;
    }
});
