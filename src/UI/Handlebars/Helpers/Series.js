var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');
var _ = require('underscore');

Handlebars.registerHelper('poster', function() {

    var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
    var poster = _.where(this.images, { coverType : 'poster' });

    if (poster[0]) {
        if (!poster[0].url.match(/^https?:\/\//)) {
            return new Handlebars.SafeString('<img class="series-poster x-series-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, poster[0].url, 250)));
        } else {
            var url = poster[0].url.replace(/^https?\:/, 'https://'); //IMDb posters need https to work, k?
            return new Handlebars.SafeString('<img class="series-poster x-series-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, url)));
        }
    }

    return new Handlebars.SafeString('<img class="series-poster placeholder-image" src="{0}">'.format(placeholder));
});

Handlebars.registerHelper('remotePoster', function() {
  var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
  var poster = this.remotePoster;

  if (poster) {
      if (!poster.match(/^https?:\/\//)) {
          return new Handlebars.SafeString('<img class="series-poster x-series-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, poster, 250)));
      } else {
          var url = poster.replace(/^https?\:/, 'https://'); //IMDb posters need https to work, k?
          return new Handlebars.SafeString('<img class="series-poster x-series-poster" {0}>'.format(Handlebars.helpers.defaultImg.call(null, url)));
      }
  }

  return new Handlebars.SafeString('<img class="series-poster placeholder-image" src="{0}">'.format(placeholder));
});

Handlebars.registerHelper('traktUrl', function() {
    return 'http://trakt.tv/search/tvdb/' + this.tvdbId + '?id_type=show';
});

Handlebars.registerHelper('imdbUrl', function() {
    return 'http://imdb.com/title/' + this.imdbId;
});

Handlebars.registerHelper('tvdbUrl', function() {
    return 'http://imdb.com/title/tt' + this.imdbId;
});

Handlebars.registerHelper('tmdbUrl', function() {
    return 'https://www.themoviedb.org/movie/' + this.tmdbId;
});

Handlebars.registerHelper('youTubeTrailerUrl', function() {
    return 'https://www.youtube.com/watch?v=' + this.youTubeTrailerId;
});

Handlebars.registerHelper('homepage', function() {
    return this.website;
});

Handlebars.registerHelper('alternativeTitlesString', function() {
  var titles = this.alternativeTitles;
  if (titles.length == 0) {
    return "";
  }
  if (titles.length == 1) {
    return titles[0];
  }
  return titles.slice(0,titles.length-1).join(", ") + " and " + titles[titles.length-1];
});

Handlebars.registerHelper('GetStatus', function() {
  var monitored = this.monitored;
  var status = this.status;
  var inCinemas = this.inCinemas;
  var date = new Date(inCinemas);
  var timeSince = new Date().getTime() - date.getTime();
  var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;


  if (status === "announced") {
    return new Handlebars.SafeString('<i class="icon-sonarr-movie-announced grid-icon" title=""></i>&nbsp;Announced');
  }

  if (numOfMonths < 3) {

    return new Handlebars.SafeString('<i class="icon-sonarr-movie-cinemas grid-icon" title=""></i>&nbsp;In Cinemas');
  }

  if (numOfMonths > 3) {
    return new Handlebars.SafeString('<i class="icon-sonarr-movie-released grid-icon" title=""></i>&nbsp;Released');//TODO: Update for PreDB.me
  }

  if (status === 'released') {
      return new Handlebars.SafeString('<i class="icon-sonarr-movie-released grid-icon" title=""></i>&nbsp;Released');
  }

  else if (!monitored) {
      return new Handlebars.SafeString('<i class="icon-sonarr-series-unmonitored grid-icon" title=""></i>&nbsp;Not Monitored');
  }
})

Handlebars.registerHelper('GetBannerStatus', function() {
  var monitored = this.monitored;
  var status = this.status;
  var inCinemas = this.inCinemas;
  var date = new Date(inCinemas);
  var timeSince = new Date().getTime() - date.getTime();
  var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;

  if (status === "announced") {
    return new Handlebars.SafeString('<div class="announced-banner"><i class="icon-sonarr-movie-announced grid-icon" title=""></i>&nbsp;Announced</div>');
  }

  if (numOfMonths < 3) {
    return new Handlebars.SafeString('<div class="cinemas-banner"><i class="icon-sonarr-movie-cinemas grid-icon" title=""></i>&nbsp;In Cinemas</div>');
  }

  if (status === 'released') {
      return new Handlebars.SafeString('<div class="released-banner"><i class="icon-sonarr-movie-released grid-icon" title=""></i>&nbsp;Released</div>');
  }

  if (numOfMonths > 3) {
    return new Handlebars.SafeString('<div class="released-banner"><i class="icon-sonarr-movie-released grid-icon" title=""></i>&nbsp;Released</div>');//TODO: Update for PreDB.me
  }




  else if (!monitored) {
      return new Handlebars.SafeString('<div class="announced-banner"><i class="icon-sonarr-series-unmonitored grid-icon" title=""></i>&nbsp;Not Monitored</div>');
  }
});

Handlebars.registerHelper('DownloadedStatusColor', function() {
  if (!this.monitored) {
    if (this.downloaded) {
      return "default";
    }
    return "warning";
  }

    if (this.downloaded) {
      return "success";
    }

  if (this.status != "released") {
    return "primary";
  }

  return "danger";
})

Handlebars.registerHelper('DownloadedStatus', function() {

  if (this.downloaded) {
    return "Downloaded";
  }
  if (!this.monitored) {
    return "Not Monitored";
  }


  return "Missing";
});

Handlebars.registerHelper("DownloadedQuality", function() {
  if (this.movieFile) {
    return this.movieFile.quality.quality.name;
  }

  return "";
})


Handlebars.registerHelper('inCinemas', function() {
  var monthNames = ["January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December"
];
  if (this.physicalRelease) {
    var d = new Date(this.physicalRelease);
    var day = d.getDate();
    var month = monthNames[d.getMonth()];
    var year = d.getFullYear();
    return "Available: " + day + ". " + month + " " + year;
  }
  if (this.inCinemas) {
    var cinemasDate = new Date(this.inCinemas);
    var year = cinemasDate.getFullYear();
    var month = monthNames[cinemasDate.getMonth()];
    return "In Cinemas: " + month + " " + year;
  }
  return "To be announced";
});

Handlebars.registerHelper('tvRageUrl', function() {
    return 'http://www.tvrage.com/shows/id-' + this.tvRageId;
});

Handlebars.registerHelper('tvMazeUrl', function() {
    return 'http://www.tvmaze.com/shows/' + this.tvMazeId + '/_';
});

Handlebars.registerHelper('route', function() {
    return StatusModel.get('urlBase') + '/movies/' + this.titleSlug;
});

Handlebars.registerHelper('percentOfEpisodes', function() {
    var episodeCount = this.episodeCount;
    var episodeFileCount = this.episodeFileCount;

    var percent = 100;

    if (episodeCount > 0) {
        percent = episodeFileCount / episodeCount * 100;
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

Handlebars.registerHelper('titleWithYear', function() {
    if (this.title.endsWith(' ({0})'.format(this.year))) {
        return this.title;
    }

    if (!this.year) {
        return this.title;
    }

    return new Handlebars.SafeString('{0} <span class="year">({1})</span>'.format(this.title, this.year));
});
