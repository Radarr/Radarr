var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');
var moment = require('moment');
var _ = require('underscore');

Handlebars.registerHelper('cover', function() {

    var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
    var cover = _.where(this.images, { coverType : 'cover' });

    if (cover[0]) {
        if (!cover[0].url.match(/^https?:\/\//)) {
            return new Handlebars.SafeString('<img class="album-cover x-album-cover" {0}>'.format(Handlebars.helpers.defaultImg.call(null, cover[0].url, 250)));
        } else {
            var url = cover[0].url.replace(/^https?\:/, '');
            return new Handlebars.SafeString('<img class="album-cover x-album-cover" {0}>'.format(Handlebars.helpers.defaultImg.call(null, url)));
        }
    }

    return new Handlebars.SafeString('<img class="album-cover placeholder-image" src="{0}">'.format(placeholder));
});



Handlebars.registerHelper('MBAlbumUrl', function() {
    return 'https://musicbrainz.org/release-group/' + this.mbId;
});

Handlebars.registerHelper('TADBAlbumUrl', function() {
    return 'http://www.theaudiodb.com/album/' + this.tadbId;
});

Handlebars.registerHelper('discogsAlbumUrl', function() {
    return 'https://www.discogs.com/master/' + this.discogsId;
});

Handlebars.registerHelper('allMusicAlbumUrl', function() {
    return 'http://www.allmusic.com/album/' + this.allMusicId;
});

Handlebars.registerHelper('albumYear', function() {
    return new Handlebars.SafeString('<span class="year">{0}</span>'.format(moment(this.releaseDate).format('YYYY')));
});
Handlebars.registerHelper('albumReleaseDate', function() {
    return new Handlebars.SafeString('<span class="release">{0}</span>'.format(moment(this.releaseDate).format('L')));
});
