var vent = require('../../vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var SelectMovieLayout = require('../Movie/SelectMovieLayout');

module.exports = NzbDroneCell.extend({
    className : 'movie-title-cell editable',

    events : {
        'click' : '_onClick'
    },

    render : function() {
        this.$el.empty();

        var movie = this.model.get('movie');

        if (movie)
        {
            this.$el.html(movie.title + " (" + movie.year + ")" );
        }
        else
        {
            this.$el.html("Click to select movie");
        }

        this.delegateEvents();
        return this;
    },

    _onClick : function () {
        var view = new SelectMovieLayout();

        this.listenTo(view, 'manualimport:selected:movie', this._setMovie);

        vent.trigger(vent.Commands.OpenModal2Command, view);
    },

    _setMovie : function (e) {
        if (this.model.has('movie') && e.model.id === this.model.get('movie').id) {
            return;
        }

        this.model.set({
            movie       : e.model.toJSON()
        });
    }
});