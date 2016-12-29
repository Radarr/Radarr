var NzbDroneController = require('../Shared/NzbDroneController');
var AppLayout = require('../AppLayout');
var MoviesCollection = require('./MoviesCollection');
var MoviesIndexLayout = require('./Index/MoviesIndexLayout');
var MoviesDetailsLayout = require('./Details/MoviesDetailsLayout');

module.exports = NzbDroneController.extend({
    _originalInit : NzbDroneController.prototype.initialize,

    initialize : function() {
        this.route('', this.series);
        this.route('movies', this.series);
        this.route('movies/:query', this.seriesDetails);

        this._originalInit.apply(this, arguments);
    },

    series : function() {
        this.setTitle('Movies');
        this.showMainRegion(new MoviesIndexLayout());
    },

    seriesDetails : function(query) {
        var series = MoviesCollection.where({ titleSlug : query });

        if (series.length !== 0) {
            var targetMovie = series[0];
            this.setTitle(targetMovie.get('title'));
            this.showMainRegion(new MoviesDetailsLayout({ model : targetMovie }));
        } else {
            this.showNotFound();
        }
    }
});
