var Marionette = require("marionette");
var Controller = require("./Controller");

module.exports = Marionette.AppRouter.extend({
    controller : new Controller(),
    appRoutes  : {
        "addseries"                  : "addSeries",
        "addseries/:action(/:query)" : "addSeries",
        "addmovies"                  : "addMovies",
        "addmovies/:action(/:query)" : "addMovies",
        "calendar"                   : "calendar",
        "settings"                   : "settings",
        "settings/:action(/:query)"  : "settings",
        "wanted"                     : "wanted",
        "wanted/:action"             : "wanted",
        "history"                    : "activity",
        "history/:action"            : "activity",
        "activity"                   : "activity",
        "activity/:action"           : "activity",
        "rss"                        : "rss",
        "system"                     : "system",
        "system/:action"             : "system",
        "seasonpass"                 : "seasonPass",
        "movieeditor"                : "movieEditor",
        ":whatever"                  : "showNotFound"
    }
});
