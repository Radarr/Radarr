var Backbone = require('backbone');
var moment = require('moment');
var MovieModel = require('../Movies/MovieModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/calendar',
    model : MovieModel,

    comparator : function(model1, model2) {

        var airDate1 = model1.get('inCinemas');
        var airDate2 = model2.get('inCinemas');
        var status1 = model1.get('status');
        var status2 = model2.get('status');

        if (status1 === 'inCinemas') {
            airDate1 = model1.get('physicalRelease');
        }

        if (status2 === 'inCinemas') {
            airDate2 = model2.get('physicalRelease');
        }

        var date1 = moment(airDate1);
        var time1 = date1.unix();

        var date2 = moment(airDate2);
        var time2 = date2.unix();

        if (time1 < time2) {
            return -1;
        }

        if (time1 > time2) {
            return 1;
        }

        return 0;
    }
});
