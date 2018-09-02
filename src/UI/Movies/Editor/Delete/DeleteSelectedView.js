var vent = require('vent');
var Marionette = require('marionette');
var Backbone = require('backbone');

module.exports = Marionette.ItemView.extend({
    template : 'Movies/Editor/Delete/DeleteSelectedTemplate',

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

    initialize : function(options) {
        this.movies = options.movies;
        this.templateHelpers = {
            removeCount : this.movies.length,
            fileCount : _.filter(this.movies, function(m){
                return m.get("hasFile");
            }).length
        };
    },

    removeMovie : function() {
        var self = this;
        var deleteFiles = this.ui.deleteFiles.prop('checked');
        var addExclusion = this.ui.addExclusion.prop('checked');
        this.ui.indicator.show();
        var proxy = _.extend(new Backbone.Model(), {
            id : '',

            url : window.NzbDrone.ApiRoot+'/movie/editor/delete?deleteFiles='+deleteFiles+'&addExclusion='+addExclusion,

            toJSON : function() {
                return _.pluck(self.movies, "id");
            }
        });

        proxy.save().done(function() {
            //vent.trigger(vent.Events.MovieDeleted, { series : self.model });
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
