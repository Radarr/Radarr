var MoviesIndexItemView = require('../MoviesIndexItemView');

module.exports = MoviesIndexItemView.extend({
    tagName  : 'li',
    template : 'Movies/Index/Posters/MoviePostersItemViewTemplate',

    initialize : function() {
        this.events['mouseenter .x-movie-poster-container'] = 'posterHoverAction';
        this.events['mouseleave .x-movie-poster-container'] = 'posterHoverAction';

        this.ui.controls = '.x-movie-controls';
        this.ui.title = '.x-title';
    },

    posterHoverAction : function() {
        this.ui.controls.slideToggle();
        this.ui.title.slideToggle();
    }
});
