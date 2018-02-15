/**
 * Created by leonardogalli on 13.02.18.
 */
var Backbone = require('backbone');
var _ = require('underscore');

module.exports = Backbone.Model.extend({
    urlRoot : window.NzbDrone.ApiRoot + '/qualitydefinition/test'
});