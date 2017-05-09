var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'exclusion-title-cell',

    render : function() {
        this.$el.empty();
        var title = this.model.get("movieTitle");
        var year = this.model.get("movieYear");
        var str = title;
        if (year > 1800) {
          str += " ("+year+")";
        }
        this.$el.html(str);

        return this;
    }
});
