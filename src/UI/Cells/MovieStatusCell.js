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
            this.$el.html('<i class="icon-sonarr-movie-released grid-icon" title="Released"></i>');
            this._setStatusWeight(3);
        }

        if (numOfMonths > 3) {
          this.$el.html('<i class="icon-sonarr-movie-released grid-icon" title="Released"></i>');//TODO: Update for PreDB.me
          this._setStatusWeight(2);
        }

        if (numOfMonths < 3) {
          this.$el.html('<i class="icon-sonarr-movie-cinemas grid-icon" title="In Cinemas"></i>');
          this._setStatusWeight(2);
        }

        if (status === "announced") {
          this.$el.html('<i class="icon-sonarr-movie-announced grid-icon" title="Announced"></i>');
          this._setStatusWeight(1);
        }

        // else if (!monitored) {
        //     this.$el.html('<i class="icon-sonarr-series-unmonitored grid-icon" title="Not Monitored"></i>');
        //     this._setStatusWeight(0);
        // }
        
        return this;
    },

    _setStatusWeight : function(weight) {
        this.model.set('statusWeight', weight, { silent : true });
    }
});
