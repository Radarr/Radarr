var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Backbone = require('backbone');
var ArtistCollection = require('../Artist/ArtistCollection');
require('typeahead');

vent.on(vent.Hotkeys.NavbarSearch, function() {
    $('.x-artist-search').focus();
});

var substringMatcher = function() {

    return function findMatches (q, cb) {
        var matches = _.select(ArtistCollection.toJSON(), function(artist) {
            return artist.name.toLowerCase().indexOf(q.toLowerCase()) > -1;
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
        name       : 'artist',
        displayKey : 'name',
        source     : substringMatcher()
    });

    $(this).on('typeahead:selected typeahead:autocompleted', function(e, artist) {
        this.blur();
        $(this).val('');
        Backbone.history.navigate('/artist/{0}'.format(artist.nameSlug), { trigger : true });
    });
};