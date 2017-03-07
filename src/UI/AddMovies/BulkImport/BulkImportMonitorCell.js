var Backgrid = require('backgrid');
var Config = require('../../Config');
var _ = require('underscore');
var vent = require("vent");
var TemplatedCell = require('../../Cells/TemplatedCell');
var NzbDroneCell = require("../../Cells/NzbDroneCell");
var Marionette = require('marionette');

module.exports = TemplatedCell.extend({
    className : 'monitor-cell',
    template  : 'AddMovies/BulkImport/BulkImportMonitorCell',

    _orig : TemplatedCell.prototype.initialize,
    _origRender : TemplatedCell.prototype.initialize,

    ui : {
      monitor : ".x-monitor",
    },

    events: { "change .x-monitor" : "_monitorChanged" },

    initialize : function () {
        this._orig.apply(this, arguments);

        this.listenTo(vent, Config.Events.ConfigUpdatedEvent, this._onConfigUpdated);

        this.defaultMonitor = Config.getValue(Config.Keys.MonitorEpisodes, 'all');

        this.model.set('monitored', this._convertMonitorToBool(this.defaultMonitor));

        this.$el.find('.x-monitor').val(this.defaultMonitor);
        // this.ui.monitor.val(this.defaultProfile);//this.ui.profile.val(this.defaultProfile);
        // this.model.set("profileId", this.defaultProfile);

        // this.cellValue = ProfileCollection;


        //this.render();
        //this.listenTo(ProfileCollection, 'sync', this.render);

    },

    _convertMonitorToBool : function(monitorString) {
        return monitorString === 'all' ? true : false;
    },

    _monitorChanged : function() {
      Config.setValue(Config.Keys.MonitorEpisodes, this.$el.find('.x-monitor').val());
      this.defaultMonitor = this.$el.find('.x-monitor').val();
      this.model.set("monitored", this._convertMonitorToBool(this.$el.find('.x-monitor').val()));
    },

    _onConfigUpdated : function(options) {
      if (options.key === Config.Keys.MonitorEpisodes) {
        this.$el.find('.x-monitor').val(options.value);
      }
    },

    render : function() {
      var templateName = this.column.get('template') || this.template;

    //   this.cellValue = ProfileCollection;

      this.templateFunction = Marionette.TemplateCache.get(templateName);
      this.$el.empty();

      if (this.cellValue) {
          var data = this.cellValue.toJSON();
          var html = this.templateFunction(data);
          this.$el.html(html);
      }

      this.delegateEvents();

      this.$el.find('.x-monitor').val(this.defaultMonitor);

      return this;
    }

});
