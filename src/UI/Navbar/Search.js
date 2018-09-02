var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Backbone = require('backbone');
var FullMovieCollection = require('../Movies/FullMovieCollection');
require('typeahead');

vent.on(vent.Hotkeys.NavbarSearch, function() {
    $('.x-movies-search').focus();
});

var substringMatcher = function() {
    return function findMatches (q, cb) {
        var matches = _.select(FullMovieCollection.toJSON(), function(movie) {
            return movie.title.toLowerCase().indexOf(q.toLowerCase()) > -1;
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
        name       : 'movie',
        displayKey : function(movie) {
           return movie.title + ' (' + movie.year + ')';
        },
        templates  : {
          empty : function(input) {
            var escapedQuery = _.escape(input.query);

            return "<div class='tt-dataset-series'><span class='tt-suggestions' style='display: block;'><div class='tt-suggestion'><p style='white-space: normal;'><a class='no-movies-found' href='/addmovies/search/'" + escapedQuery + "'>Search for " + escapedQuery + "</a></p></div></span></div>";
          },
        },
        source     : substringMatcher()
    });

    $(this).on('typeahead:selected typeahead:autocompleted', function(e, movie) {
        this.blur();
        $(this).val('');
        Backbone.history.navigate('/movies/{0}'.format(movie.titleSlug), { trigger : true });
    });
};
