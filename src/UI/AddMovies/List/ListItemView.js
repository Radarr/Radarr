var _ = require('underscore');
var vent = require('vent');
var AppLayout = require('../../AppLayout');
var Backbone = require('backbone');
var Marionette = require('marionette');
var Config = require('../../Config');
var Messenger = require('../../Shared/Messenger');
var AsValidatedView = require('../../Mixins/AsValidatedView');

require('jquery.dotdotdot');

var view = Marionette.ItemView.extend({

		template : 'AddMovies/SearchResultViewTemplate',


})


AsValidatedView.apply(view);

module.exports = view;
