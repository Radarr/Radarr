var Backbone = require('backbone');
var _ = require('underscore');

module.exports = Backbone.Model.extend({
    urlRoot : window.NzbDrone.ApiRoot + '/altyear',
});
