var Backbone = require('backbone');
var _ = require('underscore');

module.exports = Backbone.Model.extend({
    urlRoot : window.NzbDrone.ApiRoot + '/movie',

    defaults : {
        episodeFileCount : 0,
        episodeCount     : 0,
        isExisting       : false,
        status           : 0
    }
});
