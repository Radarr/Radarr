var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var params = new URLSearchParams(window.location.search.slice(1));
var oauth=params.get('oauth');
var refresh=params.get('refresh');
var q = window.location;
var callback_url = q.protocol+'//'+q.hostname+(q.port ? ':' + q.port : '')+'/settings/netimport';
history.pushState('object', 'title', callback_url);
var view = Marionette.ItemView.extend({
	template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',
	events : {
		'click .x-reset-trakt-tokens' : '_resetTraktTokens',
		'click .x-revoke-trakt-tokens' : '_revokeTraktTokens'
	},

        initialize : function() {

	},
	
	onShow : function() {
		if (oauth && refresh){
	        this.ui.authToken.val(oauth).trigger('change');
		this.ui.refreshToken.val(refresh).trigger('change');
		this.ui.tokenExpiry.val(Math.floor(Date.now() / 1000) + 4838400).trigger('change');  // this means the token will expire in 8 weeks (4838400 seconds)
	        //this.model.isSaved = false;
		}
		if (this.ui.authToken.val() && this.ui.refreshToken.val()){
                this.ui.resetTokensButton.hide();
		this.ui.revokeTokensButton.show();
		} else {
		this.ui.resetTokensButton.show();
		this.ui.revokeTokensButton.hide();
		}
	        
	},

	ui : {
		resetTraktTokens : '.x-reset-trakt-tokens',
		authToken : '.x-trakt-auth-token',
		refreshToken : '.x-trakt-refresh-token',
		resetTokensButton : '.x-reset-trakt-tokens',
		revokeTokensButton : '.x-revoke-trakt-tokens',
                tokenExpiry : '.x-trakt-token-expiry'
	},

	_resetTraktTokens : function() {
		if (window.confirm("You will now be taken to trakt.tv for authentication.\nYou will then be directed back here.\nDon't forget to click save when you get back!")){
		window.location='https://api.couchpota.to/authorize/trakt/?target='+callback_url;
		//this.ui.resetTokensButton.hide();
		}
	},

	_revokeTraktTokens : function() {
		if (window.confirm("Your tokens will be revoked. Don't forget to click save!")){
                //TODO: need to implement this: http://docs.trakt.apiary.io/#reference/authentication-oauth/revoke-token/revoke-an-access_token
	        this.ui.authToken.val('').trigger('change');
		this.ui.refreshToken.val('').trigger('change');
		this.ui.tokenExpiry.val(0).trigger('change');
	        this.ui.resetTokensButton.show();
		this.ui.revokeTokensButton.hide();
		}
	}

});


AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;