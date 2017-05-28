var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'artist-status-cell',

    render : function() {
        this.$el.empty();
        var monitored = this.model.get('monitored');
        var status = this.model.get('status');

        if (status === 'ended') {
            this.$el.html('<i class="icon-lidarr-artist-ended grid-icon" title="Ended"></i>');
            this._setStatusWeight(3);
        }

        else if (!monitored) {
            this.$el.html('<i class="icon-lidarr-artist-unmonitored grid-icon" title="Not Monitored"></i>');
            this._setStatusWeight(2);
        }

        else {
            this.$el.html('<i class="icon-lidarr-artist-continuing grid-icon" title="Continuing"></i>');
            this._setStatusWeight(1);
        }

        return this;
    },

    _setStatusWeight : function(weight) {
        this.model.set('statusWeight', weight, { silent : true });
    }
});