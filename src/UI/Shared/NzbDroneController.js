var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var NotFoundView = require('./NotFoundView');
var Messenger = require('./Messenger');
var Config = require('../Config');

module.exports = Marionette.AppRouter.extend({
    initialize : function() {
        this.listenTo(vent, vent.Events.ServerUpdated, this._onServerUpdated);
    },

    showNotFound : function() {
        this.setTitle('Not Found');
        this.showMainRegion(new NotFoundView(this));
    },

    setTitle : function(title) {
        title = title;
        if (title === 'Radarr') {
            document.title = 'Radarr';
        } else {
            document.title = title + ' - Radarr';
        }

        if (window.NzbDrone.Analytics && window.Piwik) {
            try {
                var piwik = window.Piwik.getTracker(window.location.protocol + '//radarr.video/piwik/piwik.php', 1);
                piwik.setReferrerUrl('');
                piwik.setCustomUrl('http://local' + window.location.pathname);
                piwik.setCustomVariable(1, 'version', window.NzbDrone.Version, 'page');
                piwik.setCustomVariable(2, 'branch', window.NzbDrone.Branch, 'page');
                piwik.trackPageView(title);
            }
            catch (e) {
                console.error(e);
            }
        }
    },

    _onServerUpdated : function() {
        var label = window.location.pathname === window.NzbDrone.UrlBase + '/system/updates' ? 'Reload' : 'View Changes';

        Messenger.show({
            message   : 'Radarr has been updated, some UI configuration has been reset',
            hideAfter : 0,
            id        : 'sonarrUpdated',
            actions   : {
                viewChanges : {
                    label  : label,
                    action : function() {
                        window.location = window.NzbDrone.UrlBase + '/system/updates';
                    }
                }
            }
        });

        // Only for pre-release development
        var pageSize = Config.getValue("pageSize");
        window.localStorage.clear();
        Config.setValue("pageSize", pageSize);
        // Remove above when out of pre-release :)

        this.pendingUpdate = true;
    },

    showMainRegion : function(view) {
        if (this.pendingUpdate) {
            window.location.reload();
        } else {
            AppLayout.mainRegion.show(view);
        }
    }
});
