var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');
var FormatHelpers = require('../../Shared/FormatHelpers');
var moment = require('moment');
var _ = require('underscore');
require('../../Activity/Queue/QueueCollection');

Handlebars.registerHelper('GetStatus', function() {
    var monitored = this.monitored;
    var status = this.status;
    //var inCinemas = this.inCinemas;
    //var date = new Date(inCinemas);
    //var timeSince = new Date().getTime() - date.getTime();
    //var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;


    if (status === "announced") {
      return new Handlebars.SafeString('<i class="icon-sonarr-movie-announced grid-icon" title=""></i>&nbsp;Announced');
    }


    if (status ==="inCinemas") {
      return new Handlebars.SafeString('<i class="icon-sonarr-movie-cinemas grid-icon" title=""></i>&nbsp;In Cinemas');
    }

    if (status === 'released') {
        return new Handlebars.SafeString('<i class="icon-sonarr-movie-released grid-icon" title=""></i>&nbsp;Released');
    }

    if (!monitored) {
        return new Handlebars.SafeString('<i class="icon-sonarr-series-unmonitored grid-icon" title=""></i>&nbsp;Not Monitored');
    }
  });

Handlebars.registerHelper('route', function() {
    return StatusModel.get('urlBase') + '/movies/' + this.titleSlug;
});

Handlebars.registerHelper('StatusLevel', function() {
    var hasFile = this.hasFile;
    var downloading = require('../../Activity/Queue/QueueCollection').findMovie(this.id) || this.downloading;
    var currentTime = moment();
    var monitored = this.monitored;

    if (hasFile) {
        return 'success';
    }

    else if (downloading) {
        return 'purple';
    }

    else if (!monitored) {
        return 'unmonitored';
    }

    else if (this.status === "inCinemas") {
        return 'premiere';
    }

    else if (this.status === "released") {
        return 'danger';
    }

    else if (this.status === "announced") {
        return 'primary';
    }

    return 'primary';
});

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
    return 'http://trakt.tv/search/tmdb/' + this.tmdbId + '?id_type=movie';
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
  if (titles.length === 0) {
    return "";
  }

  titles = _.map(titles, function(item){
      return item.title;
  });

  if (titles.length === 1) {
    return '"' + titles[0] + '"';
  }
  return '"' + titles.slice(0,titles.length-1).join('", "') + '" and "' + titles[titles.length-1] + '"';
});

Handlebars.registerHelper('GetBannerStatus', function() {
  var monitored = this.monitored;
  var status = this.status;
  //var inCinemas = this.inCinemas;
  //var date = new Date(inCinemas);
  //var timeSince = new Date().getTime() - date.getTime();
  //var numOfMonths = timeSince / 1000 / 60 / 60 / 24 / 30;


  if (status === "inCinemas") {
    return new Handlebars.SafeString('<div class="cinemas-banner"><i class="icon-sonarr-movie-cinemas grid-icon" title=""></i>&nbsp;In Cinemas</div>');
  }

  if (status === "announced") {
    return new Handlebars.SafeString('<div class="announced-banner"><i class="icon-sonarr-movie-announced grid-icon" title=""></i>&nbsp;Announced</div>');
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

  if (!this.isAvailable){
    return "primary";
  }

  return "danger";
});

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
});

Handlebars.registerHelper('inCinemas', function() {
  var monthNames = ["January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December"
];
  var year, month;

  if (this.physicalRelease) {
    var d = new Date(this.physicalRelease);
    var day = d.getDate();
    month = monthNames[d.getMonth()];
    year = d.getFullYear();
    return "Available: " + day + ". " + month + " " + year;
  }
  if (this.inCinemas) {
    var cinemasDate = new Date(this.inCinemas);
    year = cinemasDate.getFullYear();
    month = monthNames[cinemasDate.getMonth()];
    return "In Cinemas: " + month + " " + year;
  }
  return "To be announced";
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
