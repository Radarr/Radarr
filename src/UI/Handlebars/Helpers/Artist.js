var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');
var _ = require('underscore');

Handlebars.registerHelper('poster', function() {

    var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
    var poster = _.where(this.images, { coverType : 'poster' });

    if (poster[0]) {
        if (!poster[0].url.match(/^https?:\/\//)) {
            return new Handlebars.SafeString('<img class="artist-poster x-artist-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, poster[0].url, 250)));
        } else {
            var url = poster[0].url.replace(/^https?\:/, '');
            return new Handlebars.SafeString('<img class="artist-poster x-artist-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, url)));
        }
    }

    return new Handlebars.SafeString('<img class="artist-poster placeholder-image" src="{0}">'.format(placeholder));
});



Handlebars.registerHelper('MBUrl', function() {
    return 'https://musicbrainz.org/artist/' + this.mbId;
});

Handlebars.registerHelper('TADBUrl', function() {
    return 'http://www.theaudiodb.com/artist/' + this.tadbId;
});

Handlebars.registerHelper('discogsUrl', function() {
    return 'https://www.discogs.com/artist/' + this.discogsId;
});

Handlebars.registerHelper('allMusicUrl', function() {
    return 'http://www.allmusic.com/artist/' + this.allMusicId;
});

Handlebars.registerHelper('route', function() {
    return StatusModel.get('urlBase') + '/artist/' + this.nameSlug;
});

// Handlebars.registerHelper('percentOfEpisodes', function() {
//     var episodeCount = this.episodeCount;
//     var episodeFileCount = this.episodeFileCount;

//     var percent = 100;

//     if (episodeCount > 0) {
//         percent = episodeFileCount / episodeCount * 100;
//     }

//     return percent;
// });

Handlebars.registerHelper('percentOfTracks', function() {
    var trackCount = this.trackCount;
    var trackFileCount = this.trackFileCount;

    var percent = 100;

    if (trackCount > 0) {
        percent = trackFileCount / trackCount * 100;
    }

    return percent;
});

Handlebars.registerHelper('seasonCountHelper', function() {
    var seasonCount = this.seasonCount;
    var continuing = this.status === 'continuing';

    if (continuing) {
        return new Handlebars.SafeString('<span class="label label-info">Season {0}</span>'.format(seasonCount));
    }

    if (seasonCount === 1) {
        return new Handlebars.SafeString('<span class="label label-info">{0} Season</span>'.format(seasonCount));
    }

    return new Handlebars.SafeString('<span class="label label-info">{0} Seasons</span>'.format(seasonCount));
});

Handlebars.registerHelper ('truncate', function (str, len) {
    if (str && str.length > len && str.length > 0) {
        var new_str = str + " ";
        new_str = str.substr (0, len);
        new_str = str.substr (0, new_str.lastIndexOf(" "));
        new_str = (new_str.length > 0) ? new_str : str.substr (0, len);

        return new Handlebars.SafeString ( new_str +'...' ); 
    }
    return str;
});

Handlebars.registerHelper('albumCountHelper', function() {
    var albumCount = this.albumCount;

    if (albumCount === 1) {
        return new Handlebars.SafeString('<span class="label label-info">{0} Album</span>'.format(albumCount));
    }

    return new Handlebars.SafeString('<span class="label label-info">{0} Albums</span>'.format(albumCount));
});

/*Handlebars.registerHelper('titleWithYear', function() {
    if (this.title.endsWith(' ({0})'.format(this.year))) {
        return this.title;
    }

    if (!this.year) {
        return this.title;
    }

    return new Handlebars.SafeString('{0} <span class="year">({1})</span>'.format(this.title, this.year));
});*/
