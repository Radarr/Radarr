var $ = require('jquery');
var Backbone = require('backbone');
var Marionette = require('marionette');
var SystemInfoLayout = require('./Info/SystemInfoLayout');
var LogsLayout = require('./Logs/LogsLayout');
var UpdateLayout = require('./Update/UpdateLayout');
var BackupLayout = require('./Backup/BackupLayout');
var TaskLayout = require('./Task/TaskLayout');
var Messenger = require('../Shared/Messenger');
var StatusModel = require('./StatusModel');

module.exports = Marionette.Layout.extend({
    template : 'System/SystemLayoutTemplate',

    regions : {
        status  : '#status',
        logs    : '#logs',
        updates : '#updates',
        backup  : '#backup',
        tasks   : '#tasks'
    },

    ui : {
        statusTab  : '.x-status-tab',
        logsTab    : '.x-logs-tab',
        updatesTab : '.x-updates-tab',
        backupTab  : '.x-backup-tab',
        tasksTab   : '.x-tasks-tab'
    },

    events : {
        'click .x-status-tab'  : '_showStatus',
        'click .x-logs-tab'    : '_showLogs',
        'click .x-updates-tab' : '_showUpdates',
        'click .x-backup-tab'  : '_showBackup',
        'click .x-tasks-tab'   : '_showTasks',
        'click .x-shutdown'    : '_shutdown',
        'click .x-restart'     : '_restart'
    },

    initialize : function(options) {
        if (options.action) {
            this.action = options.action.toLowerCase();
        }

        this.templateHelpers = {
            authentication : StatusModel.get('authentication')
        };
    },

    onShow : function() {
        switch (this.action) {
            case 'logs':
                this._showLogs();
                break;
            case 'updates':
                this._showUpdates();
                break;
            case 'backup':
                this._showBackup();
                break;
            case 'tasks':
                this._showTasks();
                break;
            default:
                this._showStatus();
        }
    },

    _navigate : function(route) {
        Backbone.history.navigate(route, {
            trigger : true,
            replace : true
        });
    },

    _showStatus : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.status.show(new SystemInfoLayout());
        this.ui.statusTab.tab('show');
        this._navigate('system/status');
    },

    _showLogs : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.logs.show(new LogsLayout());
        this.ui.logsTab.tab('show');
        this._navigate('system/logs');
    },

    _showUpdates : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.updates.show(new UpdateLayout());
        this.ui.updatesTab.tab('show');
        this._navigate('system/updates');
    },

    _showBackup : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.backup.show(new BackupLayout());
        this.ui.backupTab.tab('show');
        this._navigate('system/backup');
    },

    _showTasks : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.tasks.show(new TaskLayout());
        this.ui.tasksTab.tab('show');
        this._navigate('system/tasks');
    },

    _shutdown : function() {
        $.ajax({
            url  : window.NzbDrone.ApiRoot + '/system/shutdown',
            type : 'POST'
        });

        Messenger.show({
            message : 'Radarr will shutdown shortly.',
            type    : 'info'
        });
    },

    _restart : function() {
        $.ajax({
            url  : window.NzbDrone.ApiRoot + '/system/restart',
            type : 'POST'
        });

        Messenger.show({
            message : 'Radarr will restart shortly.',
            type    : 'info'
        });
    }
});
