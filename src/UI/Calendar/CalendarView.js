var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var moment = require('moment');
var CalendarCollection = require('./CalendarCollection');
var UiSettings = require('../Shared/UiSettingsModel');
var QueueCollection = require('../Activity/Queue/QueueCollection');
var Config = require('../Config');

require('../Mixins/backbone.signalr.mixin');
require('fullcalendar');
require('jquery.easypiechart');

module.exports = Marionette.ItemView.extend({
    storageKey : 'calendar.view',

    initialize : function() {
        this.showUnmonitored = Config.getValue('calendar.show', 'monitored') === 'all';
        this.collection = new CalendarCollection().bindSignalR({ updateOnly : true });
        this.listenTo(this.collection, 'change', this._reloadCalendarEvents);
        this.listenTo(QueueCollection, 'sync', this._reloadCalendarEvents);
    },

    render : function() {
        this.$el.empty().fullCalendar(this._getOptions());
    },

    onShow : function() {
        this.$('.fc-today-button').click();
    },

    setShowUnmonitored : function (showUnmonitored) {
        if (this.showUnmonitored !== showUnmonitored) {
            this.showUnmonitored = showUnmonitored;
            this._getEvents(this.$el.fullCalendar('getView'));
        }
    },

    _viewRender : function(view, element) {
        if (Config.getValue(this.storageKey) !== view.name) {
            Config.setValue(this.storageKey, view.name);
        }

        this._getEvents(view);
        element.find('.fc-day-grid-container').css('height', '');
    },

    _eventRender : function(event, element) {
        element.addClass(event.statusLevel);
        element.children('.fc-content').addClass(event.statusLevel);

        if (event.downloading) {
            var progress = 100 - event.downloading.get('sizeleft') / event.downloading.get('size') * 100;
            var releaseTitle = event.downloading.get('title');
            var estimatedCompletionTime = moment(event.downloading.get('estimatedCompletionTime')).fromNow();
            var status = event.downloading.get('status').toLocaleLowerCase();
            var errorMessage = event.downloading.get('errorMessage');

            if (status === 'pending') {
                this._addStatusIcon(element, 'icon-lidarr-pending', 'Release will be processed {0}'.format(estimatedCompletionTime));
            }

            else if (errorMessage) {
                if (status === 'completed') {
                    this._addStatusIcon(element, 'icon-lidarr-import-failed', 'Import failed: {0}'.format(errorMessage));
                } else {
                    this._addStatusIcon(element, 'icon-lidarr-download-failed', 'Download failed: {0}'.format(errorMessage));
                }
            }

            else if (status === 'failed') {
                this._addStatusIcon(element, 'icon-lidarr-download-failed', 'Download failed: check download client for more details');
            }

            else if (status === 'warning') {
                this._addStatusIcon(element, 'icon-lidarr-download-warning', 'Download warning: check download client for more details');
            }

            else {
                element.find('.fc-time').after('<span class="chart pull-right" data-percent="{0}"></span>'.format(progress));

                element.find('.chart').easyPieChart({
                    barColor   : '#ffffff',
                    trackColor : false,
                    scaleColor : false,
                    lineWidth  : 2,
                    size       : 14,
                    animate    : false
                });

                element.find('.chart').tooltip({
                    title     : 'Album is downloading - {0}% {1}'.format(progress.toFixed(1), releaseTitle),
                    container : '.fc'
                });
            }
        }

        else if (event.model.get('unverifiedSceneNumbering')) {
            this._addStatusIcon(element, 'icon-lidarr-form-warning', 'Scene number hasn\'t been verified yet.');
        }

    },

    _eventAfterAllRender :  function () {
        if ($(window).width() < 768) {
            this.$('.fc-center').show();
            this.$('.calendar-title').remove();

            var title = this.$('.fc-center').html();
            var titleDiv = '<div class="calendar-title">{0}</div>'.format(title);

            this.$('.fc-toolbar').before(titleDiv);
            this.$('.fc-center').hide();
        }

        this._clearScrollBar();
    },

    _windowResize :  function () {
        this._clearScrollBar();
    },

    _getEvents : function(view) {
        var start = moment(view.start.toISOString()).toISOString();
        var end = moment(view.end.toISOString()).toISOString();

        this.$el.fullCalendar('removeEvents');

        this.collection.fetch({
            data    : {
                start       : start,
                end         : end,
                unmonitored : this.showUnmonitored
            },
            success : this._setEventData.bind(this)
        });
    },

    _setEventData : function(collection) {
        if (collection.length === 0) {
            return;
        }

        var events = [];
        var self = this;

        collection.each(function(model) {
            var albumTitle = model.get('title');
            var artistName = model.get('artist').name;
            var start = model.get('releaseDate');
            var runtime = '30';
            var end = moment(start).add('minutes', runtime).toISOString();

            var event = {
                title       : artistName + " - " + albumTitle,
                start       : moment(start),
                end         : moment(end),
                allDay      : true,
                statusLevel : self._getStatusLevel(model, end),
                downloading : QueueCollection.findEpisode(model.get('id')),
                model       : model,
                sortOrder   : 0
            };

            events.push(event);
        });

        this.$el.fullCalendar('addEventSource', events);
    },

    _getStatusLevel : function(element, endTime) {
        var hasFile = element.get('hasFile');
        var downloading = QueueCollection.findEpisode(element.get('id')) || element.get('grabbed');
        var currentTime = moment();
        var start = moment(element.get('releaseDate'));
        var end = moment(endTime);
        var monitored = element.get('artist').monitored && element.get('monitored');

        var statusLevel = 'primary';

        if (hasFile) {
            statusLevel = 'success';
        }

        else if (downloading) {
            statusLevel = 'purple';
        }

        else if (!monitored) {
            statusLevel = 'unmonitored';
        }

        else if (currentTime.isAfter(start) && currentTime.isBefore(end)) {
            statusLevel = 'warning';
        }

        else if (start.isBefore(currentTime) && !hasFile) {
            statusLevel = 'danger';
        }

        else if (element.get('episodeNumber') === 1) {
            statusLevel = 'premiere';
        }

        if (end.isBefore(currentTime.startOf('day'))) {
            statusLevel += ' past';
        }

        return statusLevel;
    },

    _reloadCalendarEvents : function() {
        this.$el.fullCalendar('removeEvents');
        this._setEventData(this.collection);
    },

    _getOptions    : function() {
        var options = {
            allDayDefault       : true,
            weekMode            : 'variable',
            firstDay            : UiSettings.get('firstDayOfWeek'),
            timeFormat          : 'h(:mm)t',
            viewRender          : this._viewRender.bind(this),
            eventRender         : this._eventRender.bind(this),
            eventAfterAllRender : this._eventAfterAllRender.bind(this),
            windowResize        : this._windowResize.bind(this),
            eventClick          : function(event) {
                vent.trigger(vent.Commands.ShowAlbumDetails, { album : event.model });
            }
        };

        if ($(window).width() < 768) {
            options.defaultView = Config.getValue(this.storageKey, 'basicDay');

            options.header = {
                left   : 'prev,next today',
                center : 'title',
                right  : 'basicWeek,listYear'
            };
        }

        else {
            options.defaultView = Config.getValue(this.storageKey, 'basicWeek');

            options.header = {
                left   : 'prev,next today',
                center : 'title',
                right  : 'month,basicWeek,listYear'
            };
        }

        options.views = {
            month: {
                titleFormat: 'MMMM YYYY',
                columnFormat: 'ddd'
            },
            week: {
                titleFormat: UiSettings.get('shortDateFormat'),
                columnFormat: UiSettings.get('calendarWeekColumnHeader')
            }


        };

        options.timeFormat = UiSettings.get('timeFormat');

        return options;
    },

    _addStatusIcon : function(element, icon, tooltip) {
        element.find('.fc-time').after('<span class="status pull-right"><i class="{0}"></i></span>'.format(icon));
        element.find('.status').tooltip({
            title     : tooltip,
            container : '.fc'
        });
    },

    _clearScrollBar : function () {
        // Remove height from calendar so we don't have another scroll bar
        this.$('.fc-day-grid-container').css('height', '');
        this.$('.fc-row.fc-widget-header').attr('style', '');
    }
});