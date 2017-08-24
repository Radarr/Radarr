var Backgrid = require('backgrid');
var Config = require('../../Config');
var _ = require('underscore');
var vent = require('vent');
var TemplatedCell = require('../../Cells/TemplatedCell');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var Marionette = require('marionette');

module.exports = TemplatedCell.extend({
    className : 'monitor-cell',
    template  : 'AddArtist/BulkImport/BulkImportMonitorCell',

    _orig : TemplatedCell.prototype.initialize,
    _origRender : TemplatedCell.prototype.initialize,

    ui : {
        monitor : '.x-monitor',
    },

    events: { 'change .x-monitor' : '_monitorChanged' },

    initialize : function () {
        this._orig.apply(this, arguments);

        this.defaultMonitor = Config.getValue(Config.Keys.MonitorEpisodes, 'all');

        this.model.set('monitored', this._convertMonitorToBool(this.defaultMonitor));

        this.$el.find('.x-monitor').val(this._convertBooltoMonitor(this.model.get('monitored')));
    },

    _convertMonitorToBool : function(monitorString) {
        return monitorString === 'all' ? true : false;
    },

    _convertBooltoMonitor : function(monitorBool) {
        return monitorBool === true ? 'all' : 'none';
    },

    _monitorChanged : function() {
        Config.setValue(Config.Keys.MonitorEpisodes, this.$el.find('.x-monitor').val());
        this.defaultMonitor = this.$el.find('.x-monitor').val();
        this.model.set('monitored', this._convertMonitorToBool(this.$el.find('.x-monitor').val()));
    },

    render : function() {
        var templateName = this.column.get('template') || this.template;

        this.templateFunction = Marionette.TemplateCache.get(templateName);
        this.$el.empty();

        if (this.cellValue) {
            var data = this.cellValue.toJSON();
            var html = this.templateFunction(data);
            this.$el.html(html);
        }

        this.delegateEvents();

        this.$el.find('.x-monitor').val(this._convertBooltoMonitor(this.model.get('monitored')));

        return this;
    }

});
