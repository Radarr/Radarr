var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Delete/DeleteMovieTemplate',

    events : {
        'click .x-confirm-delete' : 'removeMovie',
        'change .x-delete-files'  : 'changeDeletedFiles'
    },

    ui : {
        deleteFiles     : '.x-delete-files',
        deleteFilesInfo : '.x-delete-files-info',
        indicator       : '.x-indicator',
        addExclusion    : '.x-add-exclusion'
    },

    removeMovie : function() {
        var self = this;
        var deleteFiles = this.ui.deleteFiles.prop('checked');
        var addExclusion = this.ui.addExclusion.prop('checked');
        this.ui.indicator.show();
        this.model.set('deleted', true); 
        this.model.destroy({
            data : { 'deleteFiles' : deleteFiles,
                     'addExclusion' : addExclusion },
            wait : true
        }).done(function() {
            vent.trigger(vent.Events.MovieDeleted, { series : self.model });
            vent.trigger(vent.Commands.CloseModalCommand);
        });
    },

    changeDeletedFiles : function() {
        var deleteFiles = this.ui.deleteFiles.prop('checked');

        if (deleteFiles) {
            this.ui.deleteFilesInfo.show();
        } else {
            this.ui.deleteFilesInfo.hide();
        }
    }
});
