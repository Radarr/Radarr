var Marionette = require('marionette');
var Backgrid = require('backgrid');
var _ = require('underscore');
var QualityDefinitionCollection = require('../../Quality/QualityDefinitionCollection');
var QualityDefinitionCollectionView = require('./Definition/QualityDefinitionCollectionView');
var QualityDefinitionTestCollection = require('../../Quality/QualityDefinitionTestCollection');
var QualityCell = require('../../Cells/QualityCell');
var MatchesCell = require('../../Quality/MatchesCell');

module.exports = Marionette.Layout.extend({
    template : 'Settings/Quality/QualityLayoutTemplate',

    regions : {
        qualityDefinition : '#quality-definition',
        matchesGrid : '#qd-matches-grid'
    },

    ui : {
      testTitle : '#test-title',
        bestMatch : '#best-match'
    },

    columns : [
        {
            name  : 'this',
            label : 'Quality',
            cell  : QualityCell,
        },
        {
            name : 'matches',
            label : 'Matches',
            cell : MatchesCell
        }
    ],

    initialize : function(options) {
        this.settings = options.settings;
        this.qualityDefinitionCollection = new QualityDefinitionCollection();
        this.qualityDefinitionCollection.fetch();
        this.qualityDefinitionTestCollection = new QualityDefinitionTestCollection();
        this.listenTo(this.qualityDefinitionTestCollection, 'sync', this._showTestResults);
        this.throttledSearch = _.debounce(this.test, 300, { trailing : true }).bind(this);
    },

    onRender : function() {
        var self = this;
        this.test({title : this.ui.testTitle.val()});
        this.ui.testTitle.keyup(function(e) {

            if (_.contains([
                    9,
                    16,
                    17,
                    18,
                    19,
                    20,
                    33,
                    34,
                    35,
                    36,
                    37,
                    38,
                    39,
                    40,
                    91,
                    92,
                    93
                ], e.keyCode)) {
                return;
            }

            
            self.throttledSearch({
                title : self.ui.testTitle.val()
            });
        });
    },

    onShow : function() {
        this.qualityDefinition.show(new QualityDefinitionCollectionView({ collection : this.qualityDefinitionCollection }));
    },

    test : function(options) {
        var title = options.title || '';
        this.qualityDefinitionTestCollection.fetch({
            data : { title : title }
        });
    },

    _showTestResults : function() {
        this.ui.bestMatch.text(this.qualityDefinitionTestCollection.bestMatch.title);
        this.matchesGrid.show(new Backgrid.Grid({
            row        : Backgrid.Row,
            columns    : this.columns,
            collection : this.qualityDefinitionTestCollection,
            className  : 'table table-hover'
        }));
    }
});