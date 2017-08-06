var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var Config = require('../Config');
require('../Form/FormBuilder');
require('bootstrap');

module.exports = Marionette.ItemView.extend({
    template : 'Release/ForceDownloadViewTemplate',
});