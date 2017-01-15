var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MoviesCollection = require('../../Movies/MoviesCollection');
var SelectRow = require('./SelectMovieRow');

module.exports = Marionette.Layout.extend({
    template  : 'ManualImport/Movie/SelectMovieLayoutTemplate',

    regions : {
        movie : '.x-movie'
    },

    ui : {
        filter : '.x-filter'
    },

    columns : [
        {
            name      : 'title',
            label     : 'Title',
            cell      : 'String',
            sortValue : 'title'
        }
    ],

    initialize : function() {
        this.movieCollection = MoviesCollection.clone();
        this._setModelCollection();

        this.listenTo(this.movieCollection, 'row:selected', this._onSelected);
        this.listenTo(this, 'modal:afterShow', this._setFocus);
    },

    onRender : function() {
        this.movieView = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.movieCollection,
            className  : 'table table-hover season-grid',
            row        : SelectRow
        });

        this.movie.show(this.movieView);
        this._setupFilter();
    },

    _setupFilter : function () {
        var self = this;

        //TODO: This should be a mixin (same as Add Series searching)
        this.ui.filter.keyup(function(e) {
            if (_.contains([
                    9,
                    16,
                    17,
                    18,
                    19,
                    20,
                    33,
                    34,
                    35,
                    36,
                    37,
                    38,
                    39,
                    40,
                    91,
                    92,
                    93
                ], e.keyCode)) {
                return;
            }

            self._filter(self.ui.filter.val());
        });
    },

    _filter : function (term) {
        this.movieCollection.setFilter(['title', term, 'contains']);
        this._setModelCollection();
    },

    _onSelected : function (e) {
        this.trigger('manualimport:selected:movie', { model: e.model });

        vent.trigger(vent.Commands.CloseModal2Command);
    },

    _setFocus : function () {
        this.ui.filter.focus();
    },
    
    _setModelCollection: function () {
        var self = this;
        
        _.each(this.movieCollection.models, function (model) {
            model.collection = self.movieCollection;
        });
    }
});
