var SettingsModelBase = require("../SettingsModelBase");

module.exports = SettingsModelBase.extend({
		url            : window.NzbDrone.ApiRoot + "/config/netimport",
		successMessage : "Net import settings saved.",
		errorMessage   : "Failed to save net import settings."
});
