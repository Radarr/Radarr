var vent = require('vent');
var Marionette = require('marionette');
var Qualities = require('../../../../Quality/QualityDefinitionCollection');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../../Mixins/AsEditModalView');
require('../../../../Mixins/TagInput');
require('../../../../Mixins/FileBrowser');

var view = Marionette.ItemView.extend({
		template : 'Movies/Files/Media/Edit/EditFileTemplate',

		ui : {
				quality : '.x-quality',
				path    : '.x-path',
				tags    : '.x-tags'
		},

		events : {

		},

		initialize : function() {
			this.qualities = new Qualities();
			var self = this;
			this.listenTo(this.qualities, 'all', this._qualitiesUpdated);
			this.qualities.fetch();

		},

		onRender : function() {
			this.ui.quality.val(this.model.get("quality").quality.id);
		},

		_onBeforeSave : function() {
				var qualityId = this.ui.quality.val();
				var quality = this.qualities.find(function(m){return m.get("quality").id === parseInt(qualityId);}).get("quality");
				var mQuality = this.model.get("quality");
				mQuality.quality = quality;
				this.model.set({ quality : mQuality });
		},

		_qualitiesUpdated : function() {
				this.templateHelpers = {};
				this.templateHelpers.qualities = this.qualities.toJSON();
				this.render();
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
