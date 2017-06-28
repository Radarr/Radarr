var Backbone = require('backbone');
var AlbumModel = require('../Artist/AlbumModel');

module.exports = Backbone.Collection.extend({
    url       : window.NzbDrone.ApiRoot + '/calendar',
    model     : AlbumModel,
    tableName : 'calendar',

    comparator : function(model) {
        var date = new Date(model.get('releaseDate'));
        var time = date.getTime();
        return time;
    }
});