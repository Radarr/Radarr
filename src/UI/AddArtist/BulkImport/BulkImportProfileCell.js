var Backgrid = require('backgrid');
var ProfileCollection = require('../../Profile/ProfileCollection');
var Config = require('../../Config');
var _ = require('underscore');

module.exports = Backgrid.SelectCell.extend({
    className : 'profile-cell',

    _orig : Backgrid.SelectCell.prototype.initialize,

    initialize : function () {
        this._orig.apply(this, arguments);

        this.defaultProfile = Config.getValue(Config.Keys.DefaultProfileId);
        if(ProfileCollection.get(this.defaultProfile))
        {
            this.profile = this.defaultProfile;
        } else {
            this.profile = ProfileCollection.get(1);
        }

        this.render();

    },

    optionValues : function() {
        return _.map(ProfileCollection.models, function(model){
            return [model.get('name'), model.get('id')+""];
        });
    }

});
