﻿var $ = require('jquery');
var _ = require('underscore');
var TagCollection = require('../Tags/TagCollection');
var TagModel = require('../Tags/TagModel');
require('bootstrap.tagsinput');

var substringMatcher = function(tags, selector) {
    return function findMatches (q, cb) {
        q = q.replace(/[^-_a-z0-9]/gi, '').toLowerCase();
        var matches = _.select(tags, function(tag) {
            return selector(tag).toLowerCase().indexOf(q) > -1;
        });
        cb(matches);
    };
};
var getExistingTags = function(tagValues) {
    return _.select(TagCollection.toJSON(), function(tag) {
        return _.contains(tagValues, tag.id);
    });
};

var testTag = function(item) {
    var tagLimitations = new RegExp('[^-_a-z0-9]', 'i');
    try {
        return !tagLimitations.test(item);
    }
    catch (e) {
        return false;
    }
};

var originalAdd = $.fn.tagsinput.Constructor.prototype.add;
var originalRemove = $.fn.tagsinput.Constructor.prototype.remove;
var originalBuild = $.fn.tagsinput.Constructor.prototype.build;

$.fn.tagsinput.Constructor.prototype.add = function(item, dontPushVal) {
    var tagCollection = this.options.tagCollection;

    if (!tagCollection) {
        originalAdd.call(this, item, dontPushVal);
        return;
    }
    var self = this;

    if (typeof item === 'string') {
        var existing = _.find(tagCollection.toJSON(), { label : item });

        if (existing) {
            originalAdd.call(this, existing, dontPushVal);
        } else if (this.options.allowNew) {
            if (item === null || item === '' || !testTag(item)) {
                return;
            }

            var newTag = new TagModel();
            newTag.set({ label : item.toLowerCase() });
            tagCollection.add(newTag);

            newTag.save().done(function() {
                item = newTag.toJSON();
                originalAdd.call(self, item, dontPushVal);
            });
        }
    } else {
        originalAdd.call(self, item, dontPushVal);
    }

    self.$input.typeahead('val', '');
};

$.fn.tagsinput.Constructor.prototype.remove = function(item, dontPushVal) {
    if (item === null) {
        return;
    }

    originalRemove.call(this, item, dontPushVal);
};

$.fn.tagsinput.Constructor.prototype.build = function(options) {
    var self = this;
    var defaults = {
        confirmKeys : [
            9,
            13,
            32,
            44,
            59
        ] //tab, enter, space, comma, semi-colon
    };

    options = $.extend({}, defaults, options);

    self.$input.on('keydown', function(event) {
        if (event.which === 9) {
            var e = $.Event('keypress');
            e.which = 9;
            self.$input.trigger(e);
            event.preventDefault();
        }
    });

    self.$input.on('focusout', function() {
        self.add(self.$input.val());
        self.$input.val('');
    });

    originalBuild.call(this, options);
};

$.fn.tagInput = function(options) {

    this.each(function () {

        var input = $(this);
        var tagInput = null;

        if (input[0].hasAttribute('tag-source')) {

            var listItems = JSON.parse(input.attr('tag-source'));

            tagInput = input.tagsinput({
                freeInput: false,
                allowNew: false,
                allowDuplicates: false,
                itemValue: 'value',
                itemText: 'name',
                tagClass : input.attr('tag-class-name') || "label label-info",
                typeaheadjs: {
                    displayKey: 'name',
                    source: substringMatcher(listItems, function (t) { return t.name; })
                }
            });

            var origValue = input.val();

            input.tagsinput('removeAll');

            if (origValue) {
                _.each(origValue.split(','), function (v) {
                    var parsed = parseInt(v);
                    var found = _.find(listItems, function (t) { return t.value === parsed; });

                    if (found) {
                        input.tagsinput('add', found);
                    }
                });
            }
        }
        else {

            options = $.extend({}, { allowNew: true }, options);

            var model = options.model;
            var property = options.property;

            tagInput = input.tagsinput({
                tagCollection: TagCollection,
                freeInput: true,
                allowNew: options.allowNew,
                itemValue: 'id',
                itemText: 'label',
                trimValue: true,
                typeaheadjs: {
                    name: 'tags',
                    displayKey: 'label',
                    source: substringMatcher(TagCollection.toJSON(), function (t) { return t.label; })
                }
            });

            //Override the free input being set to false because we're using objects
            $(tagInput)[0].options.freeInput = true;

            if (model) {
                var tags = getExistingTags(model.get(property));

                //Remove any existing tags and re-add them
                input.tagsinput('removeAll');
                _.each(tags, function (tag) {
                    input.tagsinput('add', tag);
                });
                input.tagsinput('refresh');
                input.on('itemAdded', function (event) {
                    var tags = model.get(property);
                    tags.push(event.item.id);
                    model.set(property, tags);
                });
                input.on('itemRemoved', function (event) {
                    if (!event.item) {
                        return;
                    }
                    var tags = _.without(model.get(property), event.item.id);
                    model.set(property, tags);
                });
            }
        }

    });

};
