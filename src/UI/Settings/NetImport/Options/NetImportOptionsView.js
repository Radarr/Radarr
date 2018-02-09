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
var vent = require('vent');
var https = require('https');


//if ('searchParams' in HTMLAnchorElement.prototype) {
//	var URLSearchParams = require('url-search-params-polyfill');
//}

var URLSearchParams = require('url-search-params');

var view = Marionette.ItemView.extend({
	template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',
	events : {
		'click .x-reset-trakt-tokens' : '_resetTraktTokens',
		'click .x-revoke-trakt-tokens' : '_revokeTraktTokens',
		'click .x-reset-tmdb-session-id' : '_resetTmdbSessionId',
		'click .x-revoke-tmdb-session-id' : '_revokeTmdbSessionId'
	},

  initialize : function() {

	},
	
	ui : {
		resetTraktTokens : '.x-reset-trakt-tokens',
		authToken : '.x-trakt-auth-token',
		refreshToken : '.x-trakt-refresh-token',
        tokenExpiry : '.x-trakt-token-expiry',
		tmdbSessionId : '.x-tmdb-session-id',
		tmdbRequestToken : '.x-tmdb-request-token',
		resetTmdbSessionIdButton : '.x-reset-tmdb-session-id',
		revokeTmdbSessionIdButton : '.x-revoke-tmdb-session-id'
	},

	onShow : function() {
		var params = new URLSearchParams(window.location.search);
        var oauth = params.get('access');
		var refresh=params.get('refresh');
		var request_token = params.get('request_token');
		var request_token_authorized = params.get('request_token_authorized');
		var session_id = params.get('session_id');
	    history.pushState('object', 'title', (window.location.href).replace(window.location.search, ''));
		if (oauth && refresh){
			history.pushState('object', 'title', (window.location.href).replace(window.location.search, '')); // jshint ignore:line
	        this.ui.authToken.val(oauth).trigger('change');
			this.ui.refreshToken.val(refresh).trigger('change');
			var tokenExpiry = Math.floor(Date.now() / 1000) + 4838400;
			this.ui.tokenExpiry.val(tokenExpiry).trigger('change');  // this means the token will expire in 8 weeks (4838400 seconds)
			//Config.setValue("traktTokenExpiry",tokenExpiry);
			//this.model.isSaved = false;
	        //window.alert("Trakt Authentication Complete - Click Save to make the change take effect");
			vent.trigger(vent.Commands.SaveSettings);
		}
		else if(session_id) {
			this.ui.tmdbSessionId.val(session_id).trigger('change');
			vent.trigger(vent.Commands.SaveSettings);
		}
		else if (request_token) {
			this.ui.tmdbRequestToken.val(request_token).trigger('change');
			vent.trigger(vent.Commands.SaveSettings);
			window.location="https://www.themoviedb.org/auth/access?request_token="+request_token;
		}
		else if (request_token_authorized && this.ui.tmdbRequestToken.val()){
			var obj;
			var options = {
		"method": "POST",
  		"host": "api.themoviedb.org",
  		"path": "/4/auth/access_token",
  		"withCredentials": false,
		"headers": {"Authorization": "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJlODM4OTY2NTA2NGJlYTA5NzZjOWJiNzU2OTJlYmM2OCIsInN1YiI6IjU4MTdhZmY3YzNhMzY4NzY2OTAxZmRjNCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.zOg1vwB-fVfqhmfuiz352TidSNEiJi3Ze7UHJcIGZ4w",
		"Content-Type": "application/json;charset=utf-8"}
	};

    var callback = function(response) {
		var str='';

  		response.on("data", function (chunk) {
    		str+=chunk;
  		});

  		response.on("end", function () {
			obj = JSON.parse(str);
			if (obj.success) {
				//window.alert(obj.access_token);
				window.location=window.location.href+"?session_id="+obj.access_token;
			}
			else {
				window.alert("There was an error -> Perhaps TheMovieDB.org is down?");
			}
  		});

		response.on('error', function(e) {
		  	window.alert("Got error: " + e.message);
		});
	}
	
	var req = https.request(options,callback);
	var postData = '{\"request_token\":\"'+this.ui.tmdbRequestToken.val()+'\"}';
	req.write(postData);
	req.end();
}
		//if (this.ui.tmdbSessionId.val()) {
		//	this.ui.resetTmdbSessionIdButton.hide();
		//	this.revokeTmdbSessionIdButton.show();
		//} else {
		//	this.ui.resetTmdbSessionIdButton.show();
		//	this.ui.revokeTmdbSessionIdButton.hide();
		//}
		//if (this.ui.authToken.val() && this.ui.refreshToken.val()){
        //  this.ui.resetTokensButton.hide();
		//	this.ui.revokeTokensButton.show();
		//} else {
		//	this.ui.resetTokensButton.show();
		//	this.ui.revokeTokensButton.hide();
		//}
		

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

	_revokeTmdbSessionId : function() {
		if (window.confirm("Log out of TheMovieDB.org?")) {
			this.ui.tmdbSessionId.val('').trigger('change');
			this.ui.resetTmdbSessionIdButton.show();
			this.ui.revokeTmdbSessionIdButton.hide();
			window.alert("Logged out of TheMovieDB.org - Click Save to make the change take effect");
		}
	},
	
	_resetTmdbSessionId : function() {

  var obj;
	var options = {
		"method": "POST",
  		"host": "api.themoviedb.org",
  		"path": "/4/auth/request_token",
  		"withCredentials": false,
		"headers": {"Authorization": "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJlODM4OTY2NTA2NGJlYTA5NzZjOWJiNzU2OTJlYmM2OCIsInN1YiI6IjU4MTdhZmY3YzNhMzY4NzY2OTAxZmRjNCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.zOg1vwB-fVfqhmfuiz352TidSNEiJi3Ze7UHJcIGZ4w",
		"Content-Type": "application/json;charset=utf-8"}
	};

    var callback = function(response) {
		var str='';

  		response.on("data", function (chunk) {
    		str+=chunk;
  		});

  		response.on("end", function () {
			obj = JSON.parse(str);
			if (obj.success) {
				window.location=window.location.href+'?request_token='+obj.request_token;
				//window.location="https://www.themoviedb.org/auth/access?request_token="+obj.request_token;
			}
			else {
				window.alert("There was an error -> Perhaps TheMovieDB.org is down?");
			}
  		});

		response.on('error', function(e) {
		  	window.alert("Got error: " + e.message);
		});
	}


	var req = https.request(options,callback);
	var postData = '{\"redirect_to\":\"'+window.location.href+'?request_token_authorized=1\"}';
	req.write(postData);
	req.end();
	
  }

});
AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
