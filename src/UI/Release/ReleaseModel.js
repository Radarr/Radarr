var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    downloadOk : function() {
        return this.get("mappingResult") === "success" || this.get("mappingResult") === "successLenientMapping";
    },

    forceDownloadOk : function() {
        return this.get("mappingResult") === "wrongYear" || this.get("mappingResult") === "wrongTitle";
    }
});