var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'language-cell',

    render : function() {
        this.$el.empty();

        var language = this.model.get("language");

        this.$el.html(this.toTitleCase(language));

        return this;
    },

    toTitleCase : function(str)
    {
        return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
    }


});
