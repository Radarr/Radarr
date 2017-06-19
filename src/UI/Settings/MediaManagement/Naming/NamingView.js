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
            renameTracksCheckbox     : '.x-rename-tracks',
            singleTrackExample       : '.x-single-track-example',
            namingTokenHelper        : '.x-naming-token-helper',
            artistFolderExample      : '.x-artist-folder-example',
            albumFolderExample       : '.x-album-folder-example'
        },
        events                              : {
            "change .x-rename-tracks"        : '_setFailedDownloadOptionsVisibility',
            "click .x-show-wizard"           : '_showWizard',
            "click .x-naming-token-helper a" : '_addToken'
        },
        regions                             : { basicNamingRegion : '.x-basic-naming' },
        onRender                            : function() {
            if (!this.model.get('renameTracks')) {
                this.ui.namingOptions.hide();
            }
            var basicNamingView = new BasicNamingView({ model : this.model });
            this.basicNamingRegion.show(basicNamingView);
            this.namingSampleModel = new NamingSampleModel();
            this.listenTo(this.model, 'change', this._updateSamples);
            this.listenTo(this.namingSampleModel, 'sync', this._showSamples);
            this._updateSamples();
        },
        _setFailedDownloadOptionsVisibility : function() {
            var checked = this.ui.renameTracksCheckbox.prop('checked');
            if (checked) {
                this.ui.namingOptions.slideDown();
            } else {
                this.ui.namingOptions.slideUp();
            }
        },
        _updateSamples                      : function() {
            this.namingSampleModel.fetch({ data : this.model.toJSON() });
        },
        _showSamples                        : function() {
            this.ui.singleTrackExample.html(this.namingSampleModel.get('singleTrackExample'));
            this.ui.artistFolderExample.html(this.namingSampleModel.get('artistFolderExample'));
            this.ui.albumFolderExample.html(this.namingSampleModel.get('albumFolderExample'));
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
    });
    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    return view;
}).call(this);