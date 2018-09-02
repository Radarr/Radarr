var vent = require('vent');
var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
		className : 'edit-episode-file-cell',

		events : {
				'click' : '_onClick'
		},

		render : function() {
				this.$el.empty();
				this.$el.html('<i class="icon-radarr-edit" title="Edit information about this file."></i>');

				return this;
		},

		_onClick : function() {
				var self = this;
				vent.trigger(vent.Commands.EditFileCommand, { file : this.model });
		}
});
