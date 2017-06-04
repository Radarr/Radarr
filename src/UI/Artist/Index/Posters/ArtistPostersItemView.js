var ArtistIndexItemView = require('../ArtistIndexItemView');

module.exports = ArtistIndexItemView.extend({
    tagName  : 'li',
    template : 'Artist/Index/Posters/ArtistPostersItemViewTemplate',

    initialize : function() {
        this.events['mouseenter .x-artist-poster-container'] = 'posterHoverAction';
        this.events['mouseleave .x-artist-poster-container'] = 'posterHoverAction';

        this.ui.controls = '.x-artist-controls';
        this.ui.title = '.x-title';
    },

    posterHoverAction : function() {
        this.ui.controls.slideToggle();
        this.ui.title.slideToggle();
    }
});