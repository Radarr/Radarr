var vent = require("vent");
var Marionette = require("marionette");
var CommandController = require("../../Commands/CommandController");

module.exports = Marionette.ItemView.extend({
    ui : {
        refresh : ".x-refresh"
    },

    events : {
        "click .x-edit"    : "_editSeries",
        "click .x-refresh" : "_refreshSeries"
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name     : "refreshMovie",
                seriesId : this.model.get("id")
            }
        });
    },

    _editSeries : function() {
        vent.trigger(vent.Commands.EditMovieCommand, { movie : this.model });
    },

    _refreshSeries : function() {
        CommandController.Execute("refreshMovie", {
            name     : "refreshMovie",
            movieId : this.model.id
        });
    }
});
