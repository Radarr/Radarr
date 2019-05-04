var Marionette = require('marionette');
var StatusModel = require('../System/StatusModel');
require('../Mixins/TagInput');

module.exports = Marionette.Layout.extend({
    template : 'Calendar/CalendarFeedViewTemplate',

    ui : {
        includeUnmonitored : '.x-includeUnmonitored',
        premiersOnly       : '.x-premiersOnly',
        tags               : '.x-tags',
        icalUrl            : '.x-ical-url',
        icalCopy           : '.x-ical-copy',
        icalWebCal         : '.x-ical-webcal'
    },

    events : {
        'click .x-includeUnmonitored' : '_updateUrl',
        'click .x-premiersOnly'       : '_updateUrl',
        'click .x-ical-copy'          : '_copyIcalToClipboard',
        'itemAdded .x-tags'           : '_updateUrl',
        'itemRemoved .x-tags'         : '_updateUrl'
    },

    onShow : function() {
        this._updateUrl();
        this.ui.tags.tagInput({ allowNew: false });
    },

    _copyIcalToClipboard: function () {
        this.ui.icalUrl.select();
        document.execCommand("copy");
    },

    _updateUrl : function() {
        var icalUrl = window.location.host + StatusModel.get('urlBase') + '/feed/calendar/Radarr.ics?';

        if (this.ui.includeUnmonitored.prop('checked')) {
            icalUrl += 'unmonitored=true&';
        }

        if (this.ui.premiersOnly.prop('checked')) {
            icalUrl += 'premiersOnly=true&';
        }

        if (this.ui.tags.val()) {
            icalUrl += 'tags=' + this.ui.tags.val() + '&';
        }

        icalUrl += 'apikey=' + window.NzbDrone.ApiKey;

        var icalHttpUrl = window.location.protocol + '//' + icalUrl;
        var icalWebCalUrl = 'webcal://' + icalUrl;

        this.ui.icalUrl.attr('value', icalHttpUrl);
        this.ui.icalWebCal.attr('href', icalWebCalUrl);
    }
});
