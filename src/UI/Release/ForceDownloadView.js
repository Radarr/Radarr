var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var Config = require('../Config');
var LanguageCollection = require('../Settings/Profile/Language/LanguageCollection');
var AltTitleModel = require("./AlternativeTitleModel");
var AltYearModel = require("./AlternativeYearModel");
var Messenger = require('../Shared/Messenger');
require('../Form/FormBuilder');
require('bootstrap');

module.exports = Marionette.ItemView.extend({
    template : 'Release/ForceDownloadViewTemplate',

    events : {
        'click .x-download'            : '_forceDownload',
    },

    ui : {
        titleMapping : "#title-mapping",
        yearMapping : "#year-mapping",
        language : "#language-selection",
        indicator : ".x-indicator",
    },

    initialize : function(options) {
        this.release = options.release;
        this.templateHelpers = {};

        this._configureTemplateHelpers();
    },

    onShow : function() {
        if (this.release.get("mappingResult") === "wrongYear") {
            this.ui.titleMapping.hide();
        } else {
            this.ui.yearMapping.hide();
        }
    },

    _configureTemplateHelpers : function() {
        this.templateHelpers.release = this.release.toJSON();
        this.templateHelpers.languages = LanguageCollection.toJSON();
    },

    _forceDownload : function() {
        this.ui.indicator.show();
        var self = this;

        if (this.release.get("mappingResult") === "wrongYear") {
            var altYear = new AltYearModel({
                movieId : this.release.get("suspectedMovieId"),
                year : this.release.get("year")
            });
            this.savePromise = altYear.save();
        } else {
            var altTitle = new AltTitleModel({
                movieId : this.release.get("suspectedMovieId"),
                title : this.release.get("movieTitle"),
                language : this.ui.language.val(),
            });

            this.savePromise = altTitle.save();
        }

        this.savePromise.always(function(){
            self.ui.indicator.hide();
        });

        this.savePromise.success(function(){
            self.release.save(null, {
                success : function() {
                    self.release.set('queued', true);
                    vent.trigger(vent.Commands.CloseModalCommand);
                }
            });
        });
    },
});