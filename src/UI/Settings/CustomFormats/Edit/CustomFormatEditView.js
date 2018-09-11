var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var DeleteView = require('../DeleteCustomFormatView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/AutoComplete');
require('../../../Mixins/TagInput');
require('bootstrap');
require('../FormatTagHelpers');
var Handlebars = require('handlebars');
var TestLayout = require('../CustomFormatTestLayout');

var view = Marionette.Layout.extend({
    template : 'Settings/CustomFormats/Edit/CustomFormatEditViewTemplate',

    ui: {
        tags : '.x-tags'
    },

    events : {
        'click .x-back'            : '_back'
    },

    regions : {
        testArea : '#x-test-region'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    onRender: function () {
        this.ui.tags.tagsinput({
            trimValue : true,
            allowDuplicates: false,
            tagClass : function(item) {
                var cls = "label ";
                var otherLabel = "label-" + Handlebars.helpers.formatTagLabelClass(item);
                return cls + otherLabel;
            }
        });
        var self = this;
        _.each(this.model.get("formatTags"), function(item){
            self.ui.tags.tagsinput('add', item);
        });

        this.testLayout = new TestLayout({ showLegend : false, autoTest : false });
        this.testArea.show(this.testLayout);
        this.model.testCollection = this.testLayout.qualityDefinitionTestCollection;
    },

    _onAfterSave : function() {
        this.targetCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _onAfterSaveAndAdd : function() {
        this.targetCollection.add(this.model, { merge : true });

        require('../Add/CustomFormatSchemaModal').open(this.targetCollection);
    },

    _back : function() {
        if (this.model.isNew()) {
            this.model.destroy();
        }

        require('../Add/CustomFormatSchemaModal').open(this.targetCollection);
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;
