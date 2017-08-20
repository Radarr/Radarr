var $ = require('jquery');
var Backbone = require('backbone');
var Marionette = require('marionette');
var RouteBinder = require('./jQuery/RouteBinder');
var SignalRBroadcaster = require('./Shared/SignalRBroadcaster');
var NavbarLayout = require('./Navbar/NavbarLayout');
var AppLayout = require('./AppLayout');
var MoviesController = require('./Movies/MoviesController');
var SeriesController = require('./Series/SeriesController');
var Router = require('./Router');
var ModalController = require('./Shared/Modal/ModalController');
var ControlPanelController = require('./Shared/ControlPanel/ControlPanelController');
var ServerStatusModel = require('./System/StatusModel');
var Tooltip = require('./Shared/Tooltip');
var UiSettingsController = require('./Shared/UiSettingsController');

require('./jQuery/ToTheTop');
require('./Instrumentation/StringFormat');
require('./LifeCycle');
require('./Hotkeys/Hotkeys');
require('./Shared/piwikCheck');
require('./Shared/VersionChangeMonitor');

new MoviesController();
new SeriesController();
new ModalController();
new ControlPanelController();
new Router();

var app = new Marionette.Application();

app.addInitializer(function() {
    console.log('starting application');
});

app.addInitializer(SignalRBroadcaster.appInitializer, { app : app });

app.addInitializer(Tooltip.appInitializer, { app : app });

app.addInitializer(function() {
    Backbone.history.start({
        pushState : true,
        root      : ServerStatusModel.get('urlBase')
    });
    RouteBinder.bind();
    AppLayout.navbarRegion.show(new NavbarLayout());
    $('body').addClass('started');
});

app.addInitializer(UiSettingsController.appInitializer);

app.addInitializer(function() {
    var footerText = ServerStatusModel.get('version');
    if (ServerStatusModel.get('branch') !== 'master') {
        footerText += '<br>' + ServerStatusModel.get('branch');
    }
    $('#footer-region .version').html(footerText);
});

app.start();

module.exports = app;
