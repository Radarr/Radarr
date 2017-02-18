var $ = require('jquery');
var _ = require('underscore');
var SelectAllCell = require('../../Cells/SelectAllCell');
var Backgrid = require('backgrid');
var MoviesCollection = require('../../Movies/MoviesCollection');

module.exports = SelectAllCell.extend({
    _originalRender : SelectAllCell.prototype.render,

    _originalInit : SelectAllCell.prototype.initialize,

    initialize : function() {
        this._originalInit.apply(this, arguments);

        var tmdbId = this.model.get('tmdbId');
        var existingMovie = MoviesCollection.where({ tmdbId: tmdbId });
        this.isDuplicate = existingMovie.length > 0 ? true : false;

        this.listenTo(this.model, 'change', this._refresh);
    },

    onChange : function(e) {
        if(!this.isDuplicate) {
            var checked = $(e.target).prop('checked');
            this.$el.parent().toggleClass('selected', checked);
            this.model.trigger('backgrid:selected', this.model, checked);
        } else {
            $(e.target).prop('checked', false);
        }
    },

    render : function() {
        this._originalRender.apply(this, arguments);

        this.$el.children(':first').prop('disabled', this.isDuplicate);

        return this;
    },

    _refresh: function() {
        this.render();
    }
});