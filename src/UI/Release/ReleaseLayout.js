var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ReleaseCollection = require('./ReleaseCollection');
var IndexerCell = require('../Cells/IndexerCell');
var FileSizeCell = require('../Cells/FileSizeCell');
var QualityCell = require('../Cells/QualityCell');
var ApprovalStatusCell = require('../Cells/ApprovalStatusCell');
var LoadingView = require('../Shared/LoadingView');
var EditionCell = require('../Cells/EditionCell');
var ReleaseTitleCell = require("../Cells/ReleaseTitleCell");

module.exports = Marionette.Layout.extend({
    template : 'Release/ReleaseLayoutTemplate',

    regions : {
        grid    : '#x-grid',
        toolbar : '#x-toolbar'
    },

    columns : [
        {
          name      : 'edition',
          label     : 'Edition',
          sortable  : false,
          cell      : EditionCell
        },
        {
            name     : 'indexer',
            label    : 'Indexer',
            sortable : true,
            cell     : IndexerCell
        },
        {
            name     : 'title',
            label    : 'Title',
            sortable : true,
            cell     : ReleaseTitleCell
        },
        {
            name     : 'size',
            label    : 'Size',
            sortable : true,
            cell     : FileSizeCell
        },
        {
            name     : 'quality',
            label    : 'Quality',
            sortable : true,
            cell     : QualityCell
        },
        {
            name  : 'rejections',
            label : '',
            cell  : ApprovalStatusCell,
            title : 'Release Rejected'
        }
    ],

    initialize : function() {
        this.collection = new ReleaseCollection();
        this.listenTo(this.collection, 'sync', this._showTable);
    },

    onRender : function() {
        this.grid.show(new LoadingView());
        this.collection.fetch();
    },

    _showTable : function() {
        if (!this.isClosed) {
            this.grid.show(new Backgrid.Grid({
                row        : Backgrid.Row,
                columns    : this.columns,
                collection : this.collection,
                className  : 'table table-hover'
            }));
        }
    }
});
