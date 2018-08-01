var Handlebars = require('handlebars');
var _ = require('underscore');

Handlebars.registerHelper('formatTagType', function(raw) {
    var firstLetter = raw[0].toLowerCase();
    var groupKey = "Unknown";
    switch (firstLetter)
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
        case "i":
            groupKey = "Indexer Flag";
            break;
        case "e":
            groupKey = "Edition";
            break;
    }

    return new Handlebars.SafeString(groupKey);
});

Handlebars.registerHelper('formatTagLabelClass', function(raw) {
    var groupKey = Handlebars.helpers.formatTagType(raw).string.toLowerCase();

    var labelClass = "default";

    switch (groupKey)
    {
        case "custom":
            labelClass = "warning";
            break;
        case "language":
            labelClass = "success";
            break;
        case "edition":
            labelClass = "info";
            break;
    }

    return new Handlebars.SafeString(labelClass);
});

Handlebars.registerHelper('formatTag', function(raw) {
    var ret = '';

    var labelClass = Handlebars.helpers.formatTagLabelClass(raw);

    var type = Handlebars.helpers.formatTagType(raw);

    ret = "<span class='label label-{0}' title='{1}'>{2}</span>".format(labelClass, type, raw);

    return new Handlebars.SafeString(ret);
});

Handlebars.registerHelper('infoLinkCreator', function(options) {
    var wikiRoot = options.hash.wikiRoot;
    var hash = options.hash.hash;
    var hashPrefix = options.hash.hashPrefix || "";
   return new Handlebars.SafeString("https://github.com/Radarr/Radarr/wiki/{0}#{1}{2}".format(wikiRoot, hashPrefix.toLowerCase().replace(/ /g, "-"), hash.toLowerCase().replace(/ /g, "-")));
});
