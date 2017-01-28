var $ = require('jquery');
var vent = require('./vent');

module.exports = {
    ConfigNamespace : 'Radarr.',

    Events : {
        ConfigUpdatedEvent : 'ConfigUpdatedEvent'
    },

    Keys : {
        DefaultProfileId    : 'RadarrDefaultProfileId',
        DefaultRootFolderId : 'RadarrDefaultRootFolderId',
        UseSeasonFolder     : 'RadarrUseSeasonFolder',
        DefaultSeriesType   : 'RadarrDefaultSeriesType',
        MonitorEpisodes     : 'RadarrMonitorEpisodes',
        AdvancedSettings    : 'RadarradvancedSettings'
    },

    getValueJson : function (key, defaultValue) {
        var storeKey = this.ConfigNamespace + key;
        defaultValue = defaultValue || {};

        var storeValue = window.localStorage.getItem(storeKey);

        if (!storeValue) {
            return defaultValue;
        }

        return $.parseJSON(storeValue);
    },

    getValueBoolean : function(key, defaultValue) {
        defaultValue = defaultValue || false;

        return this.getValue(key, defaultValue.toString()) === 'true';
    },

    getValue : function(key, defaultValue) {
        var storeKey = this.ConfigNamespace + key;
        var storeValue = window.localStorage.getItem(storeKey);

        if (!storeValue) {
            return defaultValue;
        }

        return storeValue.toString();
    },

    setValueJson : function(key, value) {
        return this.setValue(key, JSON.stringify(value));
    },

    setValue : function(key, value) {
        var storeKey = this.ConfigNamespace + key;
        console.log('Config: [{0}] => [{1}]'.format(storeKey, value));

        if (this.getValue(key) === value.toString()) {
            return;
        }

        try {
            window.localStorage.setItem(storeKey, value);
            vent.trigger(this.Events.ConfigUpdatedEvent, {
                key   : key,
                value : value
            });
        }
        catch (error) {
            console.error('Unable to save config: [{0}] => [{1}]'.format(storeKey, value));
        }
    }
};
