var Backbone = require('backbone');
var MovieModel = require('../Movies/MovieModel');
var _ = require('underscore');

module.exports = Backbone.Collection.extend({
    url   : "https://radarr.video/recommendations/api.php",
    model : MovieModel,

    parse : function(response) {
        var self = this;

        _.each(response, function(model) {
            model.tmdbId = model.id;
            model.id = undefined;
            model.inCinemas = model.release_date;
            model.year = model.release_year;
            model.remotePoster = "https:////image.tmdb.org/t/p/original" + model.poster_path;
            model.titleSlug = model.title.toLowerCase().replace(" ", "-") + "-" + model.tmdbId;
            model.images = [
              {
                "coverType": "poster",
                "url": model.remotePoster,
              }
            ]
            model.ratings = { value : model.vote_average, votes : model.vote_count};

            var d = new Date(Date.parse(model.inCinemas));
            var today = new Date();
            var fourMonths = new Date();
            fourMonths.setMonth(fourMonths.getMonth()-4);

            if (d > today) {
              model.status = "announced";
            } else if (fourMonths > d) {
              model.status = "released";
            } else {
              model.status = "inCinemas";
            }


            if (self.unmappedFolderModel) {
                model.path = self.unmappedFolderModel.get('folder').path;
            }
        });

        return response;
    }
});
