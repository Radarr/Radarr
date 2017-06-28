var Marionette = require('marionette');
var SummaryLayout = require('./Summary/AlbumSummaryLayout');
var SearchLayout = require('./Search/AlbumSearchLayout');
var AlbumHistoryLayout = require('./History/AlbumHistoryLayout');
var ArtistCollection = require('../Artist/ArtistCollection');
var Messenger = require('../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'Album/AlbumDetailsLayoutTemplate',

    regions : {
        summary : '#album-summary',
        history : '#album-history',
        search  : '#album-search'
    },

    ui : {
        summary   : '.x-album-summary',
        history   : '.x-album-history',
        search    : '.x-album-search',
        monitored : '.x-album-monitored'
    },

    events : {

        'click .x-album-summary'   : '_showSummary',
        'click .x-album-history'   : '_showHistory',
        'click .x-album-search'    : '_showSearch',
        'click .x-album-monitored' : '_toggleMonitored'
    },

    templateHelpers : {},

    initialize : function(options) {
        
        this.templateHelpers.hideArtistLink = options.hideArtistLink;
        

        this.artist = ArtistCollection.get(this.model.get('artistId'));
        
        this.templateHelpers.artist = this.artist.toJSON();
        this.openingTab = options.openingTab || 'summary';

        this.listenTo(this.model, 'sync', this._setMonitoredState);
    },

    onShow : function() {
        this.searchLayout = new SearchLayout({ model : this.model });

        if (this.openingTab === 'search') {
            this.searchLayout.startManualSearch = true;
            this._showSearch();
        }

        else {
            this._showSummary();
        }

        this._setMonitoredState();

        if (this.artist.get('monitored')) {
            this.$el.removeClass('artist-not-monitored');
        }

        else {
            this.$el.addClass('artist-not-monitored');
        }
    },

    _showSummary : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.summary.tab('show');
        this.summary.show(new SummaryLayout({
            model  : this.model,
            artist : this.artist
        }));
    },

    _showHistory : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.history.tab('show');
        this.history.show(new AlbumHistoryLayout({
            model  : this.model,
            artist : this.artist
        }));
    },

    _showSearch : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.search.tab('show');
        this.search.show(this.searchLayout);
    },

    _toggleMonitored : function() {
        if (!this.series.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when artist is not monitored',
                type    : 'error'
            });

            return;
        }

        var name = 'monitored';
        this.model.set(name, !this.model.get(name), { silent : true });

        this.ui.monitored.addClass('icon-lidarr-spinner fa-spin');
        this.model.save();
    },

    _setMonitoredState : function() {
        this.ui.monitored.removeClass('fa-spin icon-lidarr-spinner');

        if (this.model.get('monitored')) {
            this.ui.monitored.addClass('icon-lidarr-monitored');
            this.ui.monitored.removeClass('icon-lidarr-unmonitored');
        } else {
            this.ui.monitored.addClass('icon-lidarr-unmonitored');
            this.ui.monitored.removeClass('icon-lidarr-monitored');
        }
    }
});