var Marionette = require('marionette');
var Controller = require('./Controller');

module.exports = Marionette.AppRouter.extend({
    controller : new Controller(),
    appRoutes  : {
        'addartist'                  : 'addArtist',
        'addartist/:action(/:query)' : 'addArtist',
        'calendar'                   : 'calendar',
        'settings'                   : 'settings',
        'settings/:action(/:query)'  : 'settings',
        'wanted'                     : 'wanted',
        'wanted/:action'             : 'wanted',
        'history'                    : 'activity',
        'history/:action'            : 'activity',
        'activity'                   : 'activity',
        'activity/:action'           : 'activity',
        'rss'                        : 'rss',
        'system'                     : 'system',
        'system/:action'             : 'system',
        'albumstudio'                : 'albumStudio',
        'artisteditor'               : 'artistEditor',
        ':whatever'                  : 'showNotFound'
    }
});