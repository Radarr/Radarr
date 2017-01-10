var TemplatedCell = require('./TemplatedCell');

module.exports = TemplatedCell.extend({
    className : 'series-title-cell',
    template  : 'Cells/SeriesTitleTemplate',


        render : function() {
           this.$el.html(this.model.get("movie").get("title")); //Hack, but somehow handlebar helper does not work.
                    debugger;
             return this;

         }
});
