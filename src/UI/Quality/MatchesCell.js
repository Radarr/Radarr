var TemplatedCell = require('../Cells/TemplatedCell');
var _ = require('underscore');

module.exports = TemplatedCell.extend({
    className : 'matches-cell',
    template  : 'Quality/MatchesCell',
    _orig : TemplatedCell.prototype.initialize,

    initialize : function() {
        this._orig.apply(this, arguments);
        var groups = {};
        _.each(this.cellValue.attributes, function(value, key, obj){
           var groupKey = key[0];
           switch (groupKey)
           {
               case "s":
                   groupKey = "Source";
                       break;
               case "r":
                   groupKey = "Resolution";
                   break;
               case "m":
                   groupKey = "Modifier";
                   break;
               case "c":
                   groupKey = "Custom";
                   break;
               case "l":
                   groupKey = "Language";
                   break;
               case "e":
                   groupKey = "Edition";
                   break;
           }


           if (groups[groupKey] === undefined) {
               groups[groupKey] = { matches : [], ok : false};
           }
            groups[groupKey].matches.push({ "value" : key.toUpperCase(), "ok" : value});
            if (value === true) {
                groups[groupKey].ok = true;
            }
        });

        this.cellValue.attributes.groups = groups;
    }
});
