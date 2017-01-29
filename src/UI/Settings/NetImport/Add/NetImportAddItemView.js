var _ = require('underscore');
var $ = require('jquery');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var EditView = require('../Edit/NetImportEditView');

module.exports = Marionette.ItemView.extend({
		template  : 'Settings/NetImport/Add/NetImportAddItemViewTemplate',
		tagName   : 'li',
		className : 'add-thingy-item',

		events : {
				'click .x-preset' : '_addPreset',
				'click'           : '_add'
		},

		initialize : function(options) {
				this.targetCollection = options.targetCollection;
		},

		_addPreset : function(e) {
				var presetName = $(e.target).closest('.x-preset').attr('data-id');
				var presetData = _.where(this.model.get('presets'), { name : presetName })[0];

				this.model.set(presetData);

				this._openEdit();
		},

		_add : function(e) {
				if ($(e.target).closest('.btn,.btn-group').length !== 0 && $(e.target).closest('.x-custom').length === 0) {
						return;
				}

				this._openEdit();
		},

		_openEdit : function() {
				this.model.set({
						id           : undefined,
						enableAuto    : this.model.get('enableAuto')
				});

				var editView = new EditView({
						model            : this.model,
						targetCollection : this.targetCollection
				});

				AppLayout.modalRegion.show(editView);
		}
});
