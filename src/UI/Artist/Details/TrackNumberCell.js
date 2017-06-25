var Marionette = require('marionette');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var reqres = require('../../reqres');
var ArtistCollection = require('../ArtistCollection');

module.exports = NzbDroneCell.extend({
    className : 'track-number-cell',
    template  : 'Artist/Details/TrackNumberCellTemplate',

    render : function() {
        this.$el.empty();
        this.$el.html(this.model.get('trackNumber'));
        
        var artist = ArtistCollection.get(this.model.get('artistId'));

        var alternateTitles = [];

        if (reqres.hasHandler(reqres.Requests.GetAlternateNameBySeasonNumber)) {
            alternateTitles = reqres.request(reqres.Requests.GetAlternateNameBySeasonNumber, this.model.get('seriesId'), this.model.get('seasonNumber'), this.model.get('sceneSeasonNumber'));
        }

        if (this.model.get('sceneSeasonNumber') > 0 || this.model.get('sceneEpisodeNumber') > 0 || this.model.has('sceneAbsoluteEpisodeNumber') || alternateTitles.length > 0) {
            this.templateFunction = Marionette.TemplateCache.get(this.template);

            var json = this.model.toJSON();
            json.alternateTitles = alternateTitles;

            var html = this.templateFunction(json);

            this.$el.popover({
                content   : html,
                html      : true,
                trigger   : 'hover',
                title     : 'Scene Information',
                placement : 'right',
                container : this.$el
            });
        }

        this.delegateEvents();
        return this;
    }
});