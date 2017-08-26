var _ = require('underscore');
var Marionette = require('marionette');
var Config = require('../../../../Config');
var NamingSampleModel = require('../NamingSampleModel');
var BasicNamingModel = require('./BasicNamingModel');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');

var view = Marionette.ItemView.extend({
    template : 'Settings/MediaManagement/Naming/Basic/BasicNamingViewTemplate',

    ui : {
        namingOptions        : '.x-naming-options',
        singleTrackExample   : '.x-single-track-example'
    },

    initialize : function(options) {
        this.namingModel = options.model;
        this.model = new BasicNamingModel();

        this._parseNamingModel();

        this.listenTo(this.model, 'change', this._buildFormat);
        this.listenTo(this.namingModel, 'sync', this._parseNamingModel);
    },

    _parseNamingModel : function() {
        var standardFormat = this.namingModel.get('standardTrackFormat');

        var includeArtistName = standardFormat.match(/\{Artist[-_. ]Name\}/i);
        var includeAlbumTitle = standardFormat.match(/\{Album[-_. ]Title\}/i);
        var includeQuality = standardFormat.match(/\{Quality[-_. ]Title\}/i);
        var numberStyle = standardFormat.match(/\{track(?:\:0+)?\}/i);
        var replaceSpaces = standardFormat.indexOf(' ') === -1;
        var separator = standardFormat.match(/\}( - |\.-\.|\.| )|( - |\.-\.|\.| )\{/i);

        if (separator === null || separator[1] === '.-.') {
            separator = ' - ';
        } else {
            separator = separator[1];
        }

        if (numberStyle === null) {
            numberStyle = '{track:00}';
        } else {
            numberStyle = numberStyle[0];
        }

        this.model.set({
            includeArtistName   : includeArtistName !== null,
            includeAlbumTitle   : includeAlbumTitle !== null,
            includeQuality      : includeQuality !== null,
            numberStyle         : numberStyle,
            replaceSpaces       : replaceSpaces,
            separator           : separator
        }, { silent : true });
    },

    _buildFormat : function() {
        if (Config.getValueBoolean(Config.Keys.AdvancedSettings)) {
            return;
        }

        var standardTrackFormat = '';

        if (this.model.get('includeArtistName')) {
            if (this.model.get('replaceSpaces')) {
                standardTrackFormat += '{Artist.Name}';
            } else {
                standardTrackFormat += '{Artist Name}';
            }

            standardTrackFormat += this.model.get('separator');
        }

        if (this.model.get('includeAlbumTitle')) {
            if (this.model.get('replaceSpaces')) {
                standardTrackFormat += '{Album.Title}';
            } else {
                standardTrackFormat += '{Album Title}';
            }

            standardTrackFormat += this.model.get('separator');
        }

        standardTrackFormat += this.model.get('numberStyle');

        standardTrackFormat += this.model.get('separator');

        if (this.model.get('replaceSpaces')) {
            standardTrackFormat += '{Track.Title}';
        } else {
            standardTrackFormat += '{Track Title}';
        }
        

        if (this.model.get('includeQuality')) {
            if (this.model.get('replaceSpaces')) {
                standardTrackFormat += ' {Quality.Title}';
            } else {
                standardTrackFormat += ' {Quality Title}';
            }
        }

        if (this.model.get('replaceSpaces')) {
            standardTrackFormat = standardTrackFormat.replace(/\s/g, '.');
        }

        this.namingModel.set('standardTrackFormat', standardTrackFormat);
    }
});

module.exports = AsModelBoundView.call(view);