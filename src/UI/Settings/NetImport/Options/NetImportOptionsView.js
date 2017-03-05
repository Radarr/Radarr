var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var $ = require('jquery');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

//if ('searchParams' in HTMLAnchorElement.prototype) {
//	var URLSearchParams = require('url-search-params-polyfill');
//}

var URLSearchParams = require('url-search-params');

var q = window.location;
var callback_url = q.protocol+'//'+q.hostname+(q.port ? ':' + q.port : '')+'/settings/netimport';
var view = Marionette.ItemView.extend({
	template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',
	events : {
		'click .x-reset-trakt-tokens' : '_resetTraktTokens',
		'click .x-revoke-trakt-tokens' : '_revokeTraktTokens'
	},

        initialize : function() {

	},
	
	onShow : function() {
		var params = new URLSearchParams(window.location.search);
                var oauth = params.get('access');
		var refresh=params.get('refresh');
		if (oauth && refresh){
		history.pushState('object', 'title', callback_url);
	        this.ui.authToken.val(oauth).trigger('change');
		this.ui.refreshToken.val(refresh).trigger('change');
		this.ui.tokenExpiry.val(Math.floor(Date.now() / 1000) + 4838400).trigger('change');  // this means the token will expire in 8 weeks (4838400 seconds)
	        //this.model.isSaved = false;
	        window.alert("Trakt Authentication Complete - Click Save to make the change take effect");
		}
		if (this.ui.authToken.val() && this.ui.refreshToken.val()){
                this.ui.resetTokensButton.hide();
		this.ui.revokeTokensButton.show();
		} else {
		this.ui.resetTokensButton.show();
		this.ui.revokeTokensButton.hide();
		}
	        
	},

	onRender : function() {
		this.ui.importExclusions.tagsinput({
			trimValue : true,
			tagClass  : 'label label-danger',
			/*itemText : function(item) {
				var uri;
                var text;
                if (item.startsWith('tt')) {
                    uri = window.NzbDrone.ApiRoot + '/movies/lookup/imdb?imdbId='+item;
                }
                else {
                    uri = window.NzbDrone.ApiRoot + '/movies/lookup/tmdb?tmdbId='+item;
                }
                var promise = $.ajax({
                    url     : uri,
                    type    : 'GET',
                    async   : false,
                });
                promise.success(function(response) {
                     text=response['title']+' ('+response['year']+')';
                });

                promise.error(function(request, status, error) {
                     text=item;
                });
                return text;
			}*/
        });
        this.ui.importExclusions.on('beforeItemAdd', function(event) {
            var uri;
            if (event.item.startsWith('tt')) {
                uri = window.NzbDrone.ApiRoot + '/movies/lookup/imdb?imdbId='+event.item;
            }
            else {
                uri = window.NzbDrone.ApiRoot + '/movies/lookup/tmdb?tmdbId='+event.item;
            }
            var promise = $.ajax({
                url     : uri,
                type    : 'GET',
                async   : false,
            });
            promise.success(function(response) {
                event.cancel=false;
            });

            promise.error(function(request, status, error) {
                event.cancel = true;
                window.alert(event.item+' is not a valid! Must be valid tt#### IMDB ID or #### TMDB ID');
            });
        });
    },

	ui : {
		resetTraktTokens : '.x-reset-trakt-tokens',
		authToken : '.x-trakt-auth-token',
		refreshToken : '.x-trakt-refresh-token',
		resetTokensButton : '.x-reset-trakt-tokens',
		revokeTokensButton : '.x-revoke-trakt-tokens',
        tokenExpiry : '.x-trakt-token-expiry',
		importExclusions : '.x-import-exclusions'
	},

	_resetTraktTokens : function() {
		if (window.confirm("Proceed to trakt.tv for authentication?\nYou will then be redirected back here.")){
		window.location='http://radarr.aeonlucid.com/v1/trakt/redirect?target='+callback_url;
		//this.ui.resetTokensButton.hide();
		}
	},

	_revokeTraktTokens : function() {
		if (window.confirm("Log out of trakt.tv?")){
                //TODO: need to implement this: http://docs.trakt.apiary.io/#reference/authentication-oauth/revoke-token/revoke-an-access_token
	        this.ui.authToken.val('').trigger('change');
		this.ui.refreshToken.val('').trigger('change');
		this.ui.tokenExpiry.val(0).trigger('change');
	        this.ui.resetTokensButton.show();
		this.ui.revokeTokensButton.hide();
		window.alert("Logged out of Trakt.tv - Click Save to make the change take effect");
		}
	}
});


AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
