var TemplatedCell = require('./TemplatedCell');

module.exports = TemplatedCell.extend({
    className : 'series-title-cell',
    template  : 'Cells/SeriesTitleTemplate',


        render : function() {
           this.$el.html('<a href="movies/' + this.model.get("movie").get("titleSlug") +'">' + this.model.get("movie").get("title") + '</a>'); //Hack, but somehow handlebar helper does not work.
             return this;
         }
});
