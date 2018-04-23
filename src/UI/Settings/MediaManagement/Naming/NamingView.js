var _ = require('underscore');
var Marionette = require('marionette');
var NamingSampleModel = require('./NamingSampleModel');
var BasicNamingView = require('./Basic/BasicNamingView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

module.exports = (function() {
    var view = Marionette.Layout.extend({
        template                            : 'Settings/MediaManagement/Naming/NamingViewTemplate',
        ui                                  : {
            namingOptions            : '.x-naming-options',
            renameEpisodesCheckbox   : '.x-rename-episodes',
            replacingOptions         : '.x-replacing-options',
            replaceIllegalChars      : '.x-replace-illegal-chars',
            namingTokenHelper        : '.x-naming-token-helper',
            movieExample             : '.x-movie-example',
            movieFolderExample       : '.x-movie-folder-example'
        },
        events                              : {
            'change .x-rename-episodes'      : '_setRenameEpisodesVisibility',
            'change .x-replace-illegal-chars': '_setReplaceIllegalCharsVisibility',
            'click .x-show-wizard'           : '_showWizard',
            'click .x-naming-token-helper a' : '_addToken',
            'change .x-multi-episode-style'  : '_multiEpisodeFomatChanged'
        },
        regions                             : { basicNamingRegion : '.x-basic-naming' },
        onRender                            : function() {
            if (!this.model.get('renameEpisodes')) {
                this.ui.namingOptions.hide();
            }
            if (!this.model.get('replaceIllegalCharacters')) {
                this.ui.replacingOptions.hide();
            }
            var basicNamingView = new BasicNamingView({ model : this.model });
            this.basicNamingRegion.show(basicNamingView);
            this.namingSampleModel = new NamingSampleModel();
            this.listenTo(this.model, 'change', this._updateSamples);
            this.listenTo(this.namingSampleModel, 'sync', this._showSamples);
            this._updateSamples();
        },
        _setRenameEpisodesVisibility : function() {
            var checked = this.ui.renameEpisodesCheckbox.prop('checked');
            if (checked) {
                this.ui.namingOptions.slideDown();
            } else {
                this.ui.namingOptions.slideUp();
            }
        },
        _setReplaceIllegalCharsVisibility : function() {
            var checked = this.ui.replaceIllegalChars.prop('checked');
            if (checked) {
                this.ui.replacingOptions.slideDown();
            } else {
                this.ui.replacingOptions.slideUp();
            }
        },
        _updateSamples                      : function() {
            this.namingSampleModel.fetch({ data : this.model.toJSON() });
        },
        _showSamples                        : function() {
            this.ui.movieExample.html(this.namingSampleModel.get('movieExample'));
            this.ui.movieFolderExample.html(this.namingSampleModel.get('movieFolderExample'));
        },
        _addToken                           : function(e) {
            e.preventDefault();
            e.stopPropagation();
            var target = e.target;
            var token = '';
            var input = this.$(target).closest('.x-helper-input').children('input');
            if (this.$(target).attr('data-token')) {
                token = '{{0}}'.format(this.$(target).attr('data-token'));
            } else {
                token = this.$(target).attr('data-separator');
            }
            input.val(input.val() + token);
            input.change();
            this.ui.namingTokenHelper.removeClass('open');
            input.focus();
        },
        multiEpisodeFormatChanged           : function() {
            this.model.set('multiEpisodeStyle', this.ui.multiEpisodeStyle.val());
        }
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    return view;
}).call(this);
