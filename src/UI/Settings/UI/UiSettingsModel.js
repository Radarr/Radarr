var SettingsModelBase = require('../SettingsModelBase');
var Config = require('../../Config');

module.exports = SettingsModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/config/ui',
    successMessage : 'UI settings saved',
    errorMessage   : 'Failed to save UI settings',

    origSave : SettingsModelBase.prototype.saveSettings,
    origInit : SettingsModelBase.prototype.initialize,

    initialize : function() {
      this.set("pageSize", Config.getValue("pageSize", 250));
      this.origInit.call(this);
    },

    saveSettings : function() {
      Config.setValue("pageSize", this.get("pageSize"));
      this.origSave.call(this);
    }
});
