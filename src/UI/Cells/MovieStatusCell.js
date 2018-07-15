var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'movie-status-cell',

    render : function() {
        this.$el.empty();
        var monitored = this.model.get('monitored');
        var status = this.model.get('status');
        var inCinemas = this.model.get("inCinemas");
        var date = new Date(inCinemas);
        var timeSince = new Date().getTime() - date.getTime();
        var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;


        if (status === 'released') {
		  this.$el.html('<i class="icon-radarr-movie-released grid-icon" title="Released"></i>');
		  this._setStatusWeight(3);
		}
        if (status === 'inCinemas') {
          this.$el.html('<i class="icon-radarr-movie-cinemas grid-icon" title="In Cinemas"></i>');
          this._setStatusWeight(2);
        }

        if (status === "announced") {
          this.$el.html('<i class="icon-radarr-movie-announced grid-icon" title="Announced"></i>');
          this._setStatusWeight(1);
        }

        return this;
    },

    _setStatusWeight : function(weight) {
        this.model.set('statusWeight', weight, { silent : true });
    }
});
