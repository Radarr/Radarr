var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EmptyView = require('../Index/EmptyView');
var MoviesCollection = require('../MoviesCollection');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var DownloadedQualityCell = require('../../Cells/DownloadedQualityCell');
var ProfileCell = require('../../Cells/ProfileCell');
var SelectAllCell = require('../../Cells/SelectAllCell');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./MovieEditorFooterView');
var GridPager = require('../../Shared/Grid/Pager');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Movies/Editor/MovieEditorLayoutTemplate',

    regions : {
        seriesRegion : '#x-series-editor',
        toolbar      : '#x-toolbar',
        pagerTop : "#x-movie-pager-top",
        pager : "#x-movie-pager"
    },

    ui : {
        monitored     : '.x-monitored',
        profiles      : '.x-profiles',
        rootFolder    : '.x-root-folder',
        selectedCount : '.x-selected-count'
    },

    events : {
        'click .x-save'         : '_updateAndSave',
        'change .x-root-folder' : '_rootFolderChanged'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : MovieTitleCell,
            cellValue : 'this'
        },
        {
            name: "downloadedQuality",
            label: "Downloaded",
            cell: DownloadedQualityCell,
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'path',
            label : 'Path',
            cell  : 'string'
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        items      : [
            {
                title          : 'Update Library',
                icon           : 'icon-sonarr-refresh',
                command        : 'refreshmovie',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
        ]
    },

    initialize : function() {
        this.movieCollection = MoviesCollection.clone();
        this.movieCollection.state = MoviesCollection.state;
        this.movieCollection.bindSignalR();
        //debugger;
        this.listenTo(this.movieCollection, 'save', this.render);

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'serieseditor.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setFilter
                }
            ]
        };
    },

    onRender : function() {
        this._showToolbar();
        this._showTable();
        this._showPager();
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showPager : function(){
      var pager = new GridPager({
          columns    : this.columns,
          collection : this.movieCollection,
      });
      var pagerTop = new GridPager({
          columns    : this.columns,
          collection : this.movieCollection,
      });
      this.pager.show(pager);
      this.pagerTop.show(pagerTop);
    },

    _showTable : function() {
        if (this.movieCollection.length === 0) {
            this.seriesRegion.show(new EmptyView());
            this.toolbar.close();
            return;
        }

        this.columns[0].sortedCollection = this.movieCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.movieCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.seriesRegion.show(this.editorGrid);
        this._showFooter();
    },

    _showToolbar : function() {
        this.toolbar.show(new ToolbarLayout({
            left    : [
                this.leftSideButtons
            ],
            right   : [
                this.filteringOptions
            ],
            context : this
        }));
    },

    _showFooter : function() {
        vent.trigger(vent.Commands.OpenControlPanelCommand, new FooterView({
            editorGrid : this.editorGrid,
            collection : this.movieCollection
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.movieCollection.setFilterMode(mode);
    }
});
