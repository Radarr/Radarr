var Marionette = require('marionette');
var CustomFormatCollection = require('./CustomFormatCollection');
var TestLayout = require('./CustomFormatTestLayout');
var CollectionView = require('./CustomFormatCollectionView');


module.exports = Marionette.Layout.extend({
    template : 'Settings/CustomFormats/CustomFormatsLayout',

    regions : {
        indexers       : '#x-custom-formats-region',
        test           : '#x-custom-formats-test'
    },

    initialize : function() {
        this.indexersCollection = new CustomFormatCollection();
        this.indexersCollection.fetch();
    },

    onShow : function() {
        this.indexers.show(new CollectionView({ collection : this.indexersCollection }));
        this.test.show(new TestLayout({ showLegend : true, autoTest : true }));
    }
});
