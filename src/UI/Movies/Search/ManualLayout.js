var Marionette = require('marionette');
var Backgrid = require('backgrid');
var ReleaseTitleCell = require('../../Cells/ReleaseTitleCell');
var FileSizeCell = require('../../Cells/FileSizeCell');
var QualityCell = require('../../Cells/QualityCell');
var ApprovalStatusCell = require('../../Cells/ApprovalStatusCell');
var DownloadReportCell = require('../../Release/DownloadReportCell');
var AgeCell = require('../../Release/AgeCell');
var ProtocolCell = require('../../Release/ProtocolCell');
var PeersCell = require('../../Release/PeersCell');
var EditionCell = require('../../Cells/EditionCell');
var IndexerFlagsCell = require('../../Cells/IndexerFlagsCell');
var MultipleFormatsCell = require('../../Cells/MultipleFormatsCell');

module.exports = Marionette.Layout.extend({
    template : 'Movies/Search/ManualLayoutTemplate',

    regions : {
        grid : '#episode-release-grid'
    },

    columns : [
        {
            name  : 'protocol',
            label : 'Source',
            cell  : ProtocolCell
        },
        {
            name  : 'age',
            label : 'Age',
            cell  : AgeCell
        },
        {
            name  : 'title',
            label : 'Title',
            cell  : ReleaseTitleCell
        },
        {
            name  : 'edition',
            label : 'Edition',
            cell  : EditionCell,
            title : "Edition",
        },
        {
          name : 'flags',
          label : 'Flags',
          cell : IndexerFlagsCell,
        },
        {
            name  : 'indexer',
            label : 'Indexer',
            cell  : Backgrid.StringCell
        },
        {
            name  : 'size',
            label : 'Size',
            cell  : FileSizeCell
        },
        {
            name  : 'seeders',
            label : 'Peers',
            cell  : PeersCell
        },
        {
            name  : 'quality',
            label : 'Quality',
            cell  : QualityCell,
        },
        {
            name : 'quality',
            label : 'Custom Formats',
            cell : MultipleFormatsCell
        },
        {
            name      : 'rejections',
            label     : '<i class="icon-radarr-header-rejections" />',
            tooltip   : 'Rejections',
            cell      : ApprovalStatusCell,
            sortable  : true,
            sortType  : 'fixed',
            direction : 'ascending',
            title     : 'Release Rejected'
        },
        {
            name      : 'download',
            label     : '<i class="icon-radarr-download" />',
            tooltip   : 'Auto-Search Prioritization',
            cell      : DownloadReportCell,
            sortable  : true,
            sortType  : 'fixed',
            direction : 'ascending'
        }
    ],

    onShow : function() {
        if (!this.isClosed) {
            this.grid.show(new Backgrid.Grid({
                row        : Backgrid.Row,
                columns    : this.columns,
                collection : this.collection,
                className  : 'table table-hover release-table'
            }));
        }
    }
});
