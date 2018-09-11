var Backbone = require('backbone');
var IndexerModel = require('./CustomFormatModel');

var vent = require('vent');

module.exports = Backbone.Collection.extend({
    model : IndexerModel,
    url   : window.NzbDrone.ApiRoot + '/customformat',
    
    sync : function(method, model, options) {
        vent.trigger(vent.Events.CustomFormatsChanged, {method : method});

        Backbone.Collection.prototype.sync.apply(this, arguments);
    },

    add : function(model, options) {
        vent.trigger(vent.Events.CustomFormatsChanged, {options : options});

        Backbone.Collection.prototype.add.apply(this, arguments);
    },

    remove : function(model, options) {
        vent.trigger(vent.Events.CustomFormatsChanged, {options : options});

        Backbone.Collection.prototype.remove.apply(this, arguments);
    }
});

