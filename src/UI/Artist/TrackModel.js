var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    defaults : {
        albumId      : 0,
        status       : 0
    },

    methodUrls : {
        'update' : window.NzbDrone.ApiRoot + '/track'
    },

    sync : function(method, model, options) {
        if (model.methodUrls && model.methodUrls[method.toLowerCase()]) {
            options = options || {};
            options.url = model.methodUrls[method.toLowerCase()];
        }
        return Backbone.sync(method, model, options);
    }
});