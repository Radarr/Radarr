var Marionette = require('marionette');
var Backgrid = require('backgrid');
var Backbone = require('backbone');

var CustomFormatTestCollection = require('./CustomFormatTestCollection');
var QualityCell = require('../../Cells/CustomFormatCell');
var MatchesCell = require('./MatchesCell');
var MultipleFormatsCell = require('../../Cells/MultipleFormatsCell');

module.exports = Marionette.Layout.extend({
    template : 'Settings/CustomFormats/CustomFormatTestLayout',

    regions : {
        matchesGrid : '#qd-matches-grid',
        matchedFormats : '#matched-formats'
    },

    events : {
        'change #test-title' : '_changeTestTitle'
    },

    ui : {
        testTitle : '#test-title'
    },

    columns : [
        {
            name  : 'customFormat',
            label : 'Custom Format',
            cell  : QualityCell,
        },
        {
            name : 'this',
            label : 'Matches',
            cell : MatchesCell
        }
    ],

    initialize : function(options) {
        this.options = options;
        this.templateHelpers = this.options;
        this.qualityDefinitionTestCollection = new CustomFormatTestCollection();
        this.listenTo(this.qualityDefinitionTestCollection, 'sync', this._showTestResults);
        this.throttledSearch = _.debounce(this.test, 300, { trailing : true }).bind(this);
    },

    onRender : function() {
        var self = this;

        this.qualityDefinitionTestCollection.title = this.ui.testTitle.val();

        if (this.options.autoTest === true) {
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
        }

    },

    test : function(options) {
        var title = options.title || '';
        this.qualityDefinitionTestCollection.fetch({
            data : { title : title }
        });
    },

    _showTestResults : function() {
        var model = new Backbone.Model({
            customFormats : this.qualityDefinitionTestCollection.matchedFormats
        });

        var cell = new MultipleFormatsCell({
            column: {
                name: 'this'
            },
            model: model
        });

        console.log(cell);

        this.matchedFormats.show(cell);

        this.matchesGrid.show(new Backgrid.Grid({
            row        : Backgrid.Row,
            columns    : this.columns,
            collection : this.qualityDefinitionTestCollection,
            className  : 'table table-hover'
        }));
    },

    _changeTestTitle : function() {
        this.qualityDefinitionTestCollection.title = this.ui.testTitle.val();
    }

});
