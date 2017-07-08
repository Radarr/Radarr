var _ = require('underscore');
var ToggleCell = require('./ToggleCell');
var ArtistCollection = require('../Artist/ArtistCollection');
var Messenger = require('../Shared/Messenger');

module.exports = ToggleCell.extend({
    className : 'toggle-cell track-monitored',

    _originalOnClick : ToggleCell.prototype._onClick,

    _onClick : function(e) {

        var artist = ArtistCollection.get(this.model.get('artistId'));

        if (!artist.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when artist is not monitored',
                type    : 'error'
            });

            return;
        }

        if (e.shiftKey && this.model.trackCollection.lastToggled) {
            this._selectRange();

            return;
        }

        this._originalOnClick.apply(this, arguments);
        this.model.trackCollection.lastToggled = this.model;
    },

    _selectRange : function() {
        var trackCollection = this.model.trackCollection;
        var lastToggled = trackCollection.lastToggled;

        var currentIndex = trackCollection.indexOf(this.model);
        var lastIndex = trackCollection.indexOf(lastToggled);

        var low = Math.min(currentIndex, lastIndex);
        var high = Math.max(currentIndex, lastIndex);
        var range = _.range(low + 1, high);

        _.each(range, function(index) {
            var model = trackCollection.at(index);

            model.set('monitored', lastToggled.get('monitored'));
            model.save();
        });

        this.model.set('monitored', lastToggled.get('monitored'));
        this.model.save();
        this.model.trackCollection.lastToggled = undefined;
    }
});