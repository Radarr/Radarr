var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var CommandController = require('../../Commands/CommandController');

module.exports = NzbDroneCell.extend({
		className : 'tmdbId-cell',

        // Should maybe list for Enter as well?
        events : {
            'blur input.tmdbId-input' : '_updateId'
        },

        render : function() {
            this.$el.empty();

            this.$el.html('<input type="text" class="tmdbId-input form-control" value="' + this.cellValue.get('tmdbId') + '" />');
            
            return this;
        },

        _updateId : function() {
            console.log('TODO Update Id');
            //Should we use a command for this? Is there a better way?
            // CommandController.Execute('updateTmdbId', {
            //     name     : 'updateTmdbId',
            //     movieId : this.cellValue.get('id'),
            //     tmdbId : this.cellValue.get('tmdbId')
            // });
        }
});
