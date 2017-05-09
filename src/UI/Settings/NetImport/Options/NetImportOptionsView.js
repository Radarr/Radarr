var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var ImportExclusionsCollection = require('./../ImportExclusionsCollection');
var SelectAllCell = require('../../../Cells/SelectAllCell');
var _ = require('underscore');
var vent = require('vent');
var Backgrid = require('backgrid');
var $ = require('jquery');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

var Config = require('../../../Config');


//if ('searchParams' in HTMLAnchorElement.prototype) {
//	var URLSearchParams = require('url-search-params-polyfill');
//}

var URLSearchParams = require('url-search-params');

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
			//var callback_url = window.location.href;
			history.pushState('object', 'title', (window.location.href).replace(window.location.search, '')); // jshint ignore:line
	        this.ui.authToken.val(oauth).trigger('change');
			this.ui.refreshToken.val(refresh).trigger('change');
			//Config.setValue("traktAuthToken", oauth);
			//Config.setValue("traktRefreshToken", refresh);
			var tokenExpiry = Math.floor(Date.now() / 1000) + 4838400;
			this.ui.tokenExpiry.val(tokenExpiry).trigger('change');  // this means the token will expire in 8 weeks (4838400 seconds)
			//Config.setValue("traktTokenExpiry",tokenExpiry);
			//this.model.isSaved = false;
	        //window.alert("Trakt Authentication Complete - Click Save to make the change take effect");
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
		window.location='http://radarr.aeonlucid.com/v1/trakt/redirect?target='+window.location.href;
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
	},

});


AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
