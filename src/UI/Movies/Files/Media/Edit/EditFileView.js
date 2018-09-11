var vent = require('vent');
var Marionette = require('marionette');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../../Mixins/AsEditModalView');
var LoadingView = require('../../../../Shared/LoadingView');
var ProfileSchemaCollection = require('../../../../Settings/Profile/ProfileSchemaCollection');
var SelectQualityView = require('../../../../ManualImport/Quality/SelectQualityView');

var view = Marionette.Layout.extend({
		template : 'Movies/Files/Media/Edit/EditFileTemplate',

		ui : {
				quality : '.x-quality',
				path    : '.x-path',
				tags    : '.x-tags'
		},

        regions : {
		    selectQuality : '#select-quality'
        },

		events : {

		},

        initialize : function() {
            this.profileSchemaCollection = new ProfileSchemaCollection();
            this.profileSchemaCollection.fetch();

            this.listenTo(this.profileSchemaCollection, 'sync', this._showQuality);
        },

        onRender : function() {
            this.selectQuality.show(new LoadingView());
        },

        _showQuality : function () {
            var qualities = _.map(this.profileSchemaCollection.first().get('items'), function (quality) {
                return quality.quality;
            });
            var formats = _.map(this.profileSchemaCollection.first().get('formatItems'), function (format) {
                return format.format;
            });

            var quality = this.model.get("quality");

            this.selectQualityView = new SelectQualityView({ qualities: qualities, formats : formats, current : {
                    formats : quality.customFormats, quality : quality.quality
                }
            });
            this.selectQuality.show(this.selectQualityView);
        },

		_onBeforeSave : function() {
				this.model.set({ quality : this.selectQualityView.selectedQuality() });
		},

		_onAfterSave : function() {
				this.trigger('saved');
				vent.trigger(vent.Commands.MovieFileEdited);
				vent.trigger(vent.Commands.CloseModalCommand);
		},

});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;
