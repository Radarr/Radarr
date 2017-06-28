var Marionette = require('marionette');
var Backgrid = require('backgrid');
var HistoryCollection = require('../../Activity/History/HistoryCollection');
var EventTypeCell = require('../../Cells/EventTypeCell');
var QualityCell = require('../../Cells/QualityCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var AlbumHistoryActionsCell = require('./AlbumHistoryActionsCell');
var AlbumHistoryDetailsCell = require('./AlbumHistoryDetailsCell');
var NoHistoryView = require('./NoHistoryView');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'Album/History/AlbumHistoryLayoutTemplate',

    regions : {
        historyTable : '.history-table'
    },

    columns : [
        {
            name      : 'eventType',
            label     : '',
            cell      : EventTypeCell,
            cellValue : 'this'
        },
        {
            name  : 'sourceTitle',
            label : 'Source Title',
            cell  : 'string'
        },
        {
            name  : 'quality',
            label : 'Quality',
            cell  : QualityCell
        },
        {
            name  : 'date',
            label : 'Date',
            cell  : RelativeDateCell
        },
        {
            name     : 'this',
            label    : '',
            cell     : AlbumHistoryDetailsCell,
            sortable : false
        },
        {
            name     : 'this',
            label    : '',
            cell     : AlbumHistoryActionsCell,
            sortable : false
        }
    ],

    initialize : function(options) {
        this.model = options.model;
        this.artist = options.artist;

        this.collection = new HistoryCollection({
            albumId : this.model.id,
            tableName : 'albumHistory'
        });
        this.collection.fetch();
        this.listenTo(this.collection, 'sync', this._showTable);
    },

    onRender : function() {
        this.historyTable.show(new LoadingView());
    },

    _showTable : function() {
        if (this.collection.any()) {
            this.historyTable.show(new Backgrid.Grid({
                collection : this.collection,
                columns    : this.columns,
                className  : 'table table-hover table-condensed'
            }));
        }

        else {
            this.historyTable.show(new NoHistoryView());
        }
    }
});