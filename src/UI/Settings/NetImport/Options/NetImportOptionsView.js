var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var $ = require('jquery');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

var view = Marionette.ItemView.extend({
		template : 'Settings/NetImport/Options/NetImportOptionsViewTemplate',

	ui : { 
		importExclusions : '.x-import-exclusions'
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
				url		: uri,
				type	: 'GET',
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

});

AsModelBoundView.call(view);
AsValidatedView.call(view);

module.exports = view;
