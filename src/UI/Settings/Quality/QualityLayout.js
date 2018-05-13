var Marionette = require('marionette');
var _ = require('underscore');
var QualityDefinitionCollection = require('../../Quality/QualityDefinitionCollection');
var QualityDefinitionCollectionView = require('./Definition/QualityDefinitionCollectionView');


module.exports = Marionette.Layout.extend({
    template : 'Settings/Quality/QualityLayoutTemplate',

    regions : {
        qualityDefinition : '#quality-definition',
        matchesGrid : '#qd-matches-grid'
    },


    initialize : function(options) {
        this.settings = options.settings;
        this.qualityDefinitionCollection = new QualityDefinitionCollection();
        this.qualityDefinitionCollection.fetch();

    },

    onShow : function() {
        this.qualityDefinition.show(new QualityDefinitionCollectionView({ collection : this.qualityDefinitionCollection }));
    }
});
