var Marionette = require('marionette');
var ProfileCollection = require('../../Profile/ProfileCollection');
var ProfileCollectionView = require('./ProfileCollectionView');
var DelayProfileLayout = require('./Delay/DelayProfileLayout');
var DelayProfileCollection = require('./Delay/DelayProfileCollection');
var LanguageCollection = require('./Language/LanguageCollection');

var vent = require('vent');

module.exports = Marionette.Layout.extend({
    template : 'Settings/Profile/ProfileLayoutTemplate',

    regions : {
        profile      : '#profile',
        delayProfile : '#delay-profile'
    },

    initialize : function(options) {
        this.settings = options.settings;
        ProfileCollection.fetch();

        vent.on(vent.Events.CustomFormatsChanged, function(options) {
            ProfileCollection.fetch();
        });

        this.delayProfileCollection = new DelayProfileCollection();
        this.delayProfileCollection.fetch();
    },

    onShow : function() {
        this.profile.show(new ProfileCollectionView({ collection : ProfileCollection }));
        this.delayProfile.show(new DelayProfileLayout({ collection : this.delayProfileCollection }));
    }
});