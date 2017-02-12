var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var params = new URLSearchParams(window.location.search.slice(1));
var oauth=params.get('oauth');
var refresh=params.get('refresh');
	
var view = Marionette.ItemView.extend({
	template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',
	events : {
		'click .x-reset-trakt-tokens' : '_resetTraktTokens'
	},

        initialize : function() {

	},
	
	onShow : function() {
		if (oauth && refresh){
		document.getElementById("t1").value = oauth;
		document.getElementById("t2").value = refresh;
		}
	},

	ui : {
		resetTraktTokens : '.x-reset-trakt-tokens'
	},

	_resetTraktTokens : function() {
		if (window.confirm("You will now be taken to trakt.tv for authentication.\nYou will then be directed back here.\nDon't forget to click save when you get back!")){
		l = window.location;
		callback_url=l.protocol+'//'+l.hostname+(l.port ? ':' +l.port : '')+'/settings/netimport';
		window.location='https://api.couchpota.to/authorize/trakt/?target='+callback_url;
		}
	}
});


AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
