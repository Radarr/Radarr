var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var LogTimeCell = require('./LogTimeCell');
var LogLevelCell = require('./LogLevelCell');
var LogRow = require('./LogRow');
var GridPager = require('../../../Shared/Grid/Pager');
var LogCollection = require('../LogsCollection');
var ToolbarLayout = require('../../../Shared/Toolbar/ToolbarLayout');
var LoadingView = require('../../../Shared/LoadingView');
require('../../../jQuery/jquery.spin');

module.exports = Marionette.Layout.extend({
    template : 'System/Logs/Table/LogsTableLayoutTemplate',

    regions : {
        grid    : '#x-grid',
        toolbar : '#x-toolbar',
        pager   : '#x-pager'
    },

    attributes : {
        id : 'logs-screen'
    },

    columns : [
        {
            name     : 'level',
            label    : '',
            sortable : true,
            cell     : LogLevelCell
        },
        {
            name     : 'logger',
            label    : 'Component',
            sortable : true,
            cell     : Backgrid.StringCell.extend({
                className : 'log-logger-cell'
            })
        },
        {
            name     : 'message',
            label    : 'Message',
            sortable : false,
            cell     : Backgrid.StringCell.extend({
                className : 'log-message-cell'
            })
        },
        {
            name  : 'time',
            label : 'Time',
            cell  : LogTimeCell
        }
    ],

    initialize : function() {
        this.collection = new LogCollection();

        this.listenTo(this.collection, 'sync', this._showTable);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);
    },

    onRender : function() {
        this.grid.show(new LoadingView());
    },

    onShow : function() {
        this._showToolbar();
    },

    _showTable : function() {
        this.grid.show(new Backgrid.Grid({
            row        : LogRow,
            columns    : this.columns,
            collection : this.collection,
            className  : 'table table-hover'
        }));

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : this.collection
        }));
    },

    _showToolbar : function() {
        var filterButtons = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'logs.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-radarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'info',
                    title    : '',
                    tooltip  : 'Info',
                    icon     : 'icon-radarr-log-info',
                    callback : this._setFilter
                },
                {
                    key      : 'warn',
                    title    : '',
                    tooltip  : 'Warn',
                    icon     : 'icon-radarr-log-warn',
                    callback : this._setFilter
                },
                {
                    key      : 'error',
                    title    : '',
                    tooltip  : 'Error',
                    icon     : 'icon-radarr-log-error',
                    callback : this._setFilter
                }
            ]
        };

        var leftSideButtons = {
            type       : 'default',
            storeState : false,
            items      : [
                {
                    title        : 'Refresh',
                    icon         : 'icon-radarr-refresh',
                    ownerContext : this,
                    callback     : this._refreshTable
                },
                {
                    title   : 'Clear Logs',
                    icon    : 'icon-radarr-clear',
                    command : 'clearLog'
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            left    : [leftSideButtons],
            right   : [filterButtons],
            context : this
        }));
    },

    _refreshTable : function(buttonContext) {
        this.collection.state.currentPage = 1;
        var promise = this.collection.fetch({ reset : true });

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.collection.setFilterMode(mode, { reset : false });

        this.collection.state.currentPage = 1;
        var promise = this.collection.fetch({ reset : true });

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _commandComplete : function(options) {
        if (options.command.get('name') === 'clearlog') {
            this._refreshTable();
        }
    }
});