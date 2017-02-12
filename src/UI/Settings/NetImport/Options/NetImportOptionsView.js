var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var params = new URLSearchParams(window.location.search.slice(1));
var oauth=params.get('oauth');
var refresh=params.get('refresh');
var q = window.location;
var url = q.protocol+'//'+q.hostname+(q.port ? ':' + q.port : '')+'/settings/netimport';
history.pushState('object', 'title', url);
var view = Marionette.ItemView.extend({
	template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',
	events : {
		'click .x-reset-trakt-tokens' : '_resetTraktTokens'
	},

        initialize : function() {

	},
	
	onShow : function() {
		if (oauth && refresh){
	        this.ui.authToken.val(oauth).trigger('change');
		this.ui.refreshToken.val(refresh).trigger('change');;
	        //this.model.isSaved = false;
		}
	},

	ui : {
		resetTraktTokens : '.x-reset-trakt-tokens',
		authToken : '.x-trakt-auth-token',
		refreshToken : '.x-trakt-refresh-token'
	},

	_resetTraktTokens : function() {
		if (window.confirm("You will now be taken to trakt.tv for authentication.\nYou will then be directed back here.\nDon't forget to click save when you get back!")){
		var l = window.location;
		var callback_url=l.protocol+'//'+l.hostname+(l.port ? ':' +l.port : '')+'/settings/netimport';
		window.location='https://api.couchpota.to/authorize/trakt/?target='+callback_url;
		}
	}
});


AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
