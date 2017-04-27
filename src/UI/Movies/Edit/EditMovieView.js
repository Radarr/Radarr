var vent = require('vent');
var Marionette = require('marionette');
var Profiles = require('../../Profile/ProfileCollection');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../Mixins/AsValidatedView');
var AsEditModalView = require('../../Mixins/AsEditModalView');
require('../../Mixins/TagInput');
require('../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
    template : 'Movies/Edit/EditMovieTemplate',

    ui : {
        profile : '.x-profile',
        path    : '.x-path',
        tags    : '.x-tags'
    },

    events : {
        'click .x-remove' : '_removeMovie'
    },

    initialize : function() {
        this.model.set('profiles', Profiles);
        var pathState = this.model.get("pathState");
        if (pathState == "static") {
          this.model.set("pathState", true);
        } else {
          this.model.set("pathState", false);
        }
    },

    onRender : function() {
        this.ui.path.fileBrowser();
        this.ui.tags.tagInput({
            model    : this.model,
            property : 'tags'
        });

    },

    _onBeforeSave : function() {
        var profileId = this.ui.profile.val();
        this.model.set({ profileId : profileId });
        var pathState = this.model.get("pathState");
        if (pathState === true) {
          this.model.set("pathState", "static");
        } else {
          this.model.set("pathState", "dynamic");
        }
    },

    _onAfterSave : function() {
		this.model.set('saved', true);
        this.trigger('saved');
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _removeMovie : function() {
        vent.trigger(vent.Commands.DeleteMovieCommand, { movie : this.model });
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;
