var ToggleCell = require('./ToggleCell');
var Handlebars = require('handlebars');

module.exports = ToggleCell.extend({
    className : 'artist-monitored-cell',

    events : {
        'click i' : '_onClick'
    },

    render : function() {

        this.$el.empty();
        this.$el.html('<i /><a href=""><span class="artist-monitored-name"></span></a>');

        var name = this.column.get('name');

        if (this.model.get(name)) {
            this.$('i').addClass(this.column.get('trueClass'));
        } else {
            this.$('i').addClass(this.column.get('falseClass'));
        }

        var link = "/artist/" + this.model.get('nameSlug');
        var artistName = this.model.get('name');

        this.$('a').attr('href', link );
        this.$('span').html(artistName);

        var tooltip = this.column.get('tooltip');

        if (tooltip) {
            this.$('i').attr('title', tooltip);
        }

        this.delegateEvents();
        return this;
    }
});