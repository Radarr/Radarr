var _ = require('underscore');
var Config = require('../Config');

module.exports = function() {

    var originalInit = this.prototype.initialize;
    var _setInitialState, _storeStateFromBackgrid, _storeState, _convertDirectionToInt;
    this.prototype.initialize = function(options) {

        options = options || {};

        if (options.tableName) {
            this.tableName = options.tableName;
        }

        if (!this.tableName && !options.tableName) {
            throw 'tableName is required';
        }

        _setInitialState.call(this);

        this.on('backgrid:sort', _storeStateFromBackgrid, this);
        this.on('drone:sort', _storeState, this);

        if (originalInit) {
            originalInit.call(this, options);
        }
    };

    if (!this.prototype._getSortMapping) {
        this.prototype._getSortMapping = function(key) {
            return {
                name    : key,
                sortKey : key
            };
        };
    }

    _setInitialState = function() {
        var key = Config.getValue('{0}.sortKey'.format(this.tableName), this.state.sortKey);
        var direction = Config.getValue('{0}.sortDirection'.format(this.tableName), this.state.order);
        var order = parseInt(direction, 10);

        this.state.sortKey = this._getSortMapping(key).sortKey;
        this.state.order = order;
    };

    _storeStateFromBackgrid = function(column, sortDirection) {
        var order = _convertDirectionToInt(sortDirection);
        var sortKey = this._getSortMapping(column.get('name')).sortKey;

        Config.setValue('{0}.sortKey'.format(this.tableName), sortKey);
        Config.setValue('{0}.sortDirection'.format(this.tableName), order);
    };

    _storeState = function(sortModel, sortDirection) {
        var order = _convertDirectionToInt(sortDirection);
        var sortKey = this._getSortMapping(sortModel.get('name')).sortKey;

        Config.setValue('{0}.sortKey'.format(this.tableName), sortKey);
        Config.setValue('{0}.sortDirection'.format(this.tableName), order);
    };

    _convertDirectionToInt = function(dir) {
        if (dir === 'ascending') {
            return '-1';
        }

        return '1';
    };

    return this;
};
