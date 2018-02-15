var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'extra-type-cell',

    render : function() {
        this.$el.empty();

        var title = this.model.get('type');
            this.$el.html(this.toTitleCase(title));

        return this;
    },

    toTitleCase : function(str)
    {
        return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
    }
});
