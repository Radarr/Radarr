var Backbone = require("backbone");

module.exports = Backbone.Model.extend({
    defaults : {
        seasonNumber : 0,
        status       : 0
    },

    methodUrls : {
        "update" : window.NzbDrone.ApiRoot + "/episode"
    },

    sync : function(method, model, options) {
        if (model.methodUrls && model.methodUrls[method.toLowerCase()]) {
            options = options || {};
            options.url = model.methodUrls[method.toLowerCase()];
        }
        return Backbone.sync(method, model, options);
    }
});