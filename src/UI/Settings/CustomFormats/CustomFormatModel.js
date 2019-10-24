var ProviderSettingsModelBase = require('../ProviderSettingsModelBase');
var Messenger = require('../../Shared/Messenger');
var $ = require('jquery');


module.exports = ProviderSettingsModelBase.extend({
    test : function() {
        var self = this;

        this.trigger('validation:sync');

        var params = {};

        params.url = this.collection.url + '/test?title=' + encodeURIComponent(this.testCollection.title);
        params.contentType = 'application/json';
        params.data = JSON.stringify(this.toJSON());
        params.type = 'POST';
        params.isValidatedCall = true;

        var promise = $.ajax(params);

        Messenger.monitor({
            promise        : promise,
            successMessage : 'Testing \'{0}\' succeeded'.format(this.get('name')),
            errorMessage   : 'Testing \'{0}\' failed'.format(this.get('name'))
        });

        promise.fail(function(response) {
            self.trigger('validation:failed', response);
        });

        promise.done(function(response) {
            console.warn(response);
           self.testCollection.set(response, {parse:true});
            self.testCollection.trigger('sync', self.testCollection, response);
        });

        return promise;
    }
});
