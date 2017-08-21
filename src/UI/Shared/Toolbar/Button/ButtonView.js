var Backbone = require("backbone");
var Marionette = require("marionette");
var _ = require("underscore");
var CommandController = require("../../../Commands/CommandController");

module.exports = Marionette.ItemView.extend({
    template  : "Shared/Toolbar/ButtonTemplate",
    className : "btn btn-default btn-icon-only-xs",

    ui : {
        icon : "i"
    },

    events : {
        "click" : "onClick"
    },

    initialize : function() {
        this.storageKey = this.model.get("menuKey") + ":" + this.model.get("key");
    },

    onRender : function() {
        if (this.model.get("active")) {
            this.$el.addClass("active");
            this.invokeCallback();
        }

        if (!this.model.get("title")) {
            this.$el.addClass("btn-icon-only");
        }

        if (this.model.get("className")) {
            this.$el.addClass(this.model.get("className"));
        }

        if (this.model.get("tooltip")) {
            this.$el.attr("title", this.model.get("tooltip"));
        }

        var command = this.model.get("command");
        if (command) {
            var properties = _.extend({ name : command }, this.model.get("properties"));

            CommandController.bindToCommand({
                command : properties,
                element : this.$el
            });
        }
    },

    onClick : function() {
        if (this.$el.hasClass("disabled")) {
            return;
        }

        this.invokeCallback();
        this.invokeRoute();
        this.invokeCommand();
    },

    invokeCommand : function() {
        var command = this.model.get("command");
        if (command) {
            CommandController.Execute(command, this.model.get("properties"));
        }
    },

    invokeRoute : function() {
        var route = this.model.get("route");
        if (route) {
            Backbone.history.navigate(route, { trigger : true });
        }
    },

    invokeCallback : function() {
        if (!this.model.ownerContext) {
            throw "ownerContext must be set.";
        }

        var callback = this.model.get("callback");
        if (callback) {
            callback.call(this.model.ownerContext, this);
        }
    }
});