var Backgrid = require('backgrid');
var ProfileCollection = require('../../Profile/ProfileCollection');
var Config = require('../../Config');
var _ = require('underscore');
var vent = require('vent');
var TemplatedCell = require('../../Cells/TemplatedCell');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var Marionette = require('marionette');

module.exports = TemplatedCell.extend({
    className : 'profile-cell',
    template  : 'AddArtist/BulkImport/BulkImportProfileCell',

    _orig : TemplatedCell.prototype.initialize,
    _origRender : TemplatedCell.prototype.initialize,

    ui : {
        profile : '.x-profile',
    },

    events: { 'change .x-profile' : '_profileChanged' },

    initialize : function () {
        this._orig.apply(this, arguments);

        this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);

        this.defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);

        this.profile = this.defaultProfile;

        if(ProfileCollection.get(this.defaultProfile))
        {
            this.profile = this.defaultProfile;
            this.model.set('profileId', this.defaultProfile);
        } else {
            this.profile = 1;
            this.model.set('profileId', 1);
        }

        this.$('.x-profile').val(this.model.get('profileId'));

        this.cellValue = ProfileCollection;

    },

    _profileChanged : function() {
        Config.setValue(Config.Keys.DefaultProfileId, this.$('.x-profile').val());
        this.model.set('profileId', this.$('.x-profile').val());
    },

    _onConfigUpdated : function(options) {
        if (options.key === Config.Keys.DefaultProfileId) {
            this.defaultProfile = options.value;
        }
    },

    render : function() {
        var templateName = this.column.get('template') || this.template;

        this.cellValue = ProfileCollection;

        this.templateFunction = Marionette.TemplateCache.get(templateName);
        this.$el.empty();

        if (this.cellValue) {
            var data = this.cellValue.toJSON();
            var html = this.templateFunction(data);
            this.$el.html(html);
        }

        this.delegateEvents();
        this.$('.x-profile').val(this.model.get('profileId'));
        return this;
    }

});
