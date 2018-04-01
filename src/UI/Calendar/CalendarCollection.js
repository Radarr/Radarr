var Backbone = require('backbone');
var MovieModel = require('../Movies/MovieModel');

module.exports = Backbone.Collection.extend({
    url       : window.NzbDrone.ApiRoot + '/calendar',
    model     : MovieModel,
    tableName : 'calendar',

    comparator : function(model) {
        var date = new Date(model.get('inCinemas'));
        var time = date.getTime();
        return time;
    }
});
