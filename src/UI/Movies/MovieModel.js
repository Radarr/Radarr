var Backbone = require('backbone');
var _ = require('underscore');

module.exports = Backbone.Model.extend({
    urlRoot : window.NzbDrone.ApiRoot + '/movie',

    defaults : {
        episodeFileCount : 0,
        episodeCount     : 0,
        isExisting       : false,
        status           : 0
    },

    getStatus : function() {
      var monitored = this.get("monitored");
      var status = this.get("status");
      var inCinemas = this.get("inCinemas");
      var date = new Date(inCinemas);
      var timeSince = new Date().getTime() - date.getTime();
      var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;

      if (status === "announced") {
        return "announced"
      }

      if (numOfMonths < 3 && numOfMonths > 0) {

        return "inCinemas";
      }

      if (status === 'released') {
          return "released";
      }

      if (numOfMonths > 3) {
        return "released";//TODO: Update for PreDB.me
      }
    }
});
