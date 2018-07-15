var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MoviesCollection = require('../../Movies/MoviesCollection');
var SelectRow = require('./SelectMovieRow');
var FullMovieCollection = require('../../Movies/FullMovieCollection');
var Backbone = require('backbone');

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
        },
        {
            name      : 'year',
            label     : 'Year',
            cell      : 'String',
            sortValue : 'year'
        }
    ],

    initialize : function() {
        this.fullMovieCollection = FullMovieCollection;
        this.movieCollection = new Backbone.Collection(this.fullMovieCollection.first(20));
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

        //TODO: This should be a mixin (same as Add Movie searching)
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
        this.movieCollection.reset(this.fullMovieCollection.filter(function(model){
            return (model.get("title") + " "+model.get("year")+"").toLowerCase().indexOf(term.toLowerCase()) != -1;
        }).slice(0, 50));

        this._setModelCollection();
        //this.movieView.render();
    },

    _onSelected : function (e) {
        debugger;
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
