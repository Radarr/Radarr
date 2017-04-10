var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

var view = Marionette.ItemView.extend({
    template : 'Settings/Indexers/Options/IndexerOptionsViewTemplate',

    ui : {
          hcwhitelist : '.x-hcwhitelist',
        },

    onRender : function() {
			this.ui.hcwhitelist.tagsinput({
					trimValue : true,
					allowDuplicates: true,
					tagClass  : 'label label-success'
			});
		},
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
