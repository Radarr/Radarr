var Backgrid = require('backgrid');
var ProfileCollection = require('../Profile/ProfileCollection');
var _ = require('underscore');

module.exports = Backgrid.Cell.extend({
    className : 'profile-cell',

    _originalInit : Backgrid.Cell.prototype.initialize,

    initialize : function () {
        this._originalInit.apply(this, arguments);

        this.listenTo(ProfileCollection, 'sync', this.render);
    },

    render : function() {

        this.$el.empty();
        if (this.model.get("movieFile")) {
          var profileId = this.model.get("movieFile").quality.quality.id;
            this.$el.html(this.model.get("movieFile").quality.quality.name);

        }


        return this;
    }
});
