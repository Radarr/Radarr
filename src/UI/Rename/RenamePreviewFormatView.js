var vent = require('vent');
var Marionette = require('marionette');
var NamingModel = require('../Settings/MediaManagement/Naming/NamingModel');

module.exports = Marionette.ItemView.extend({
    template : 'Rename/RenamePreviewFormatViewTemplate',

    templateHelpers : function() {
        var type = this.model.get('seriesType');

        return {
            rename : this.naming.get('renameTracks'),
            format : this.naming.get('standardTrackFormat')
        };
    },

    initialize : function() {
        this.naming = new NamingModel();
        this.naming.fetch();
        this.listenTo(this.naming, 'sync', this.render);
    }
});