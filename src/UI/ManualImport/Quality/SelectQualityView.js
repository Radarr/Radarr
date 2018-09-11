var _ = require('underscore');
var Marionette = require('marionette');
var Backbone = require('backbone');
require('../../Mixins/TagInput');

module.exports = Marionette.ItemView.extend({
    template  : 'ManualImport/Quality/SelectQualityViewTemplate',

    ui : {
        select : '.x-select-quality',
        proper : 'x-proper',
        formats: '.x-tags',
    },

    initialize : function(options) {
        this.qualities = options.qualities;
        this.formats = options.formats;
        this.current = options.current || {};

        this.templateHelpers = {
            qualities: this.qualities,
            formats: JSON.stringify(_.map(this.formats, function(f) {
                return { value : f.id, name : f.name };
            })),
        };
    },

    onRender : function() {
        if (this.current.formats != undefined) {
            this.ui.formats.val(this.current.formats.map(function(m) {return m.id;}).join(","));
        }
        if (this.current.quality != undefined) {
            this.ui.select.val(this.current.quality.id);
        }
        this.ui.formats.tagInput();
    },

    selectedQuality : function () {
        var selected = parseInt(this.ui.select.val(), 10);
        var proper = this.ui.proper.prop('checked');

        var quality = _.find(this.qualities, function(q) {
            return q.id === selected;
        });

        var formatIds = this.ui.formats.val().split(',');

        var formats = _.map(_.filter(this.formats, function(f) {
            return formatIds.includes(f.id + "");
        }), function(f) {
            return { name : f.name, id : f.id};
        });

        return {
            quality  : quality,
            revision : {
                version : proper ? 2 : 1,
                real    : 0
            },
            customFormats : formats
        };
    }
});
