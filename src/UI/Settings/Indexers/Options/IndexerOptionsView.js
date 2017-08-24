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
			leniencyTooltip : '.x-leniency-tooltip',
        },

    onRender : function() {
			this.ui.hcwhitelist.tagsinput({
					trimValue : true,
					allowDuplicates: true,
					tagClass  : 'label label-success'
			});

            this.templateFunction = Marionette.TemplateCache.get('Settings/Indexers/Options/LeniencyTooltipTemplate');
            var content = this.templateFunction();

            this.ui.leniencyTooltip.popover({
                content   : content,
                html      : true,
                trigger   : 'hover',
                title     : 'Parsing Leniency Notes',
                placement : 'right',
                container : this.$el
            });
		},
});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
