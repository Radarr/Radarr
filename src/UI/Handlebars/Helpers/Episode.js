var Handlebars = require('handlebars');
var FormatHelpers = require('../../Shared/FormatHelpers');
var moment = require('moment');
require('../../Activity/Queue/QueueCollection');

Handlebars.registerHelper('EpisodeNumber', function() {

    if (this.series.seriesType === 'daily') {
        return moment(this.airDate).format('L');
    } else if (this.series.seriesType === 'anime' && this.absoluteEpisodeNumber !== undefined) {
        return '{0}x{1} ({2})'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2), FormatHelpers.pad(this.absoluteEpisodeNumber, 2));
    } else {
        return '{0}x{1}'.format(this.seasonNumber, FormatHelpers.pad(this.episodeNumber, 2));
    }
});



Handlebars.registerHelper('EpisodeProgressClass', function() {
    if (this.episodeFileCount === this.episodeCount) {
        if (this.status === 'continuing') {
            return '';
        }

        return 'progress-bar-success';
    }

    if (this.monitored) {
        return 'progress-bar-danger';
    }

    return 'progress-bar-warning';
});