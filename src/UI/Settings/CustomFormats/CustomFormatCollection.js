var Backbone = require('backbone');
var IndexerModel = require('./CustomFormatModel');

module.exports = Backbone.Collection.extend({
    model : IndexerModel,
    url   : window.NzbDrone.ApiRoot + '/customformat'
});

