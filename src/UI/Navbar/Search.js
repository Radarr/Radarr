var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Backbone = require('backbone');
//var FullMovieCollection = require('../Movies/FullMovieCollection');
var MoviesCollectionClient = require('../Movies/MoviesCollectionClient');
require('typeahead');

vent.on(vent.Hotkeys.NavbarSearch, function() {
    $('.x-series-search').focus();
});

var substringMatcher = function() {
    return function findMatches (q, cb) {
        var matches = _.select(MoviesCollectionClient.fullCollection.toJSON(), function(series) {
            return series.title.toLowerCase().indexOf(q.toLowerCase()) > -1;
        });
        cb(matches);
    };
};

$.fn.bindSearch = function() {
    $(this).typeahead({
        hint      : true,
        highlight : true,
        minLength : 1
    }, {
        name       : 'series',
        displayKey : function(series) {
           return series.title + ' (' + series.year + ')';
        },
        source     : substringMatcher()
    });

    $(this).on('typeahead:selected typeahead:autocompleted', function(e, series) {
        this.blur();
        $(this).val('');
        Backbone.history.navigate('/movies/{0}'.format(series.titleSlug), { trigger : true });
    });
};
