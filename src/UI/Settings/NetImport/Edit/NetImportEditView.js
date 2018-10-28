var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var DeleteView = require('../Delete/NetImportDeleteView');
var Profiles = require('../../../Profile/ProfileCollection');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
var RootFolders = require('../../../AddMovies/RootFolders/RootFolderCollection');
var RootFolderLayout = require('../../../AddMovies/RootFolders/RootFolderLayout');
var Config = require('../../../Config');
require('../../../Form/FormBuilder');
require('../../../Mixins/AutoComplete');
require('bootstrap');

var view = Marionette.ItemView.extend({
		template : 'Settings/NetImport/Edit/NetImportEditViewTemplate',

		ui : {
				profile         : '.x-profile',
				minimumAvailability : '.x-minimumavailability',
				rootFolder      : '.x-root-folder',
			},

		events : {
				'click .x-back'            : '_back',
				'click .x-captcha-refresh' : '_onRefreshCaptcha',
				'change .x-root-folder'   : '_rootFolderChanged',
		},

		_deleteView : DeleteView,

		initialize : function(options) {
				this.targetCollection = options.targetCollection;
				this.templateHelpers = {};

				this._configureTemplateHelpers();
				this.listenTo(this.model, 'change', this.render);
				this.listenTo(RootFolders, 'all', this._rootFoldersUpdated);
		},

		onRender : function() {
				var rootFolder = this.model.get("rootFolderPath");
				if (rootFolder !== "") {
                    this.ui.rootFolder.children().filter(function() {
                        return $.trim($(this).text()) === rootFolder;
                    }).prop('selected', true);
				} else {
					var defaultRoot = Config.getValue(Config.Keys.DefaultRootFolderId);
					if (RootFolders.get(defaultRoot)) {
							this.ui.rootFolder.val(defaultRoot);
					}
				}
		},

		_onBeforeSave : function() {
			var profile = parseInt(this.ui.profile.val(), 10);
			var minAvail = this.ui.minimumAvailability.val();
			var rootFolderPath = this.ui.rootFolder.children(':selected').text();
			this.model.set({
				profileId : profile,
				rootFolderPath : rootFolderPath,
				minimumAvailability : minAvail,
			});
		},

		_onAfterSave : function() {
				this.targetCollection.add(this.model, { merge : true });
				vent.trigger(vent.Commands.CloseModalCommand);
		},

		_onAfterSaveAndAdd : function() {
				this.targetCollection.add(this.model, { merge : true });

				require('../Add/NetImportSchemaModal').open(this.targetCollection);
		},

		_back : function() {
				if (this.model.isNew()) {
						this.model.destroy();
				}

				require('../Add/NetImportSchemaModal').open(this.targetCollection);
		},

		_configureTemplateHelpers : function() {
			this.templateHelpers.profiles = Profiles.toJSON();
			this.templateHelpers.rootFolders = RootFolders.toJSON();
		},

		_rootFolderChanged : function() {
			var rootFolderValue = this.ui.rootFolder.val();
			if (rootFolderValue === 'addNew') {
					var rootFolderLayout = new RootFolderLayout();
					this.listenToOnce(rootFolderLayout, 'folderSelected', this._setRootFolder);
					AppLayout.modalRegion.show(rootFolderLayout);
			} else {
					Config.setValue(Config.Keys.DefaultRootFolderId, rootFolderValue);
			}
		},

		_rootFoldersUpdated : function() {
				this._configureTemplateHelpers();
				this.render();
		},

		_onRefreshCaptcha : function(event) {
				var self = this;

				var target = $(event.target).parents('.input-group');

				this.ui.indicator.show();

				this.model.requestAction("checkCaptcha")
						.then(function(result) {
								if (!result.captchaRequest) {
										self.model.setFieldValue('CaptchaToken', '');

										return result;
								}

								return self._showCaptcha(target, result.captchaRequest);
						})
						.always(function() {
								self.ui.indicator.hide();
						});
		},

		_showCaptcha : function(target, captchaRequest) {
				var self = this;

				var widget = $('<div class="g-recaptcha"></div>').insertAfter(target);

				return this._loadRecaptchaWidget(widget[0], captchaRequest.siteKey, captchaRequest.secretToken)
						.then(function(captchaResponse) {
								target.parents('.form-group').removeAllErrors();
								widget.remove();

								var queryParams = {
										responseUrl    : captchaRequest.responseUrl,
										ray            : captchaRequest.ray,
										captchaResponse: captchaResponse
								};

								return self.model.requestAction("getCaptchaCookie", queryParams);
						})
						.then(function(response) {
								self.model.setFieldValue('CaptchaToken', response.captchaToken);
						});
		},

		_loadRecaptchaWidget : function(widget, sitekey, stoken) {
				var promise = $.Deferred();

				var renderWidget = function() {
						window.grecaptcha.render(widget, {
							'sitekey'  : sitekey,
							'stoken'   : stoken,
							'callback' : promise.resolve
						});
				};

				if (window.grecaptcha) {
						renderWidget();
				} else {
						window.grecaptchaLoadCallback = function() {
								delete window.grecaptchaLoadCallback;
								renderWidget();
						};

						$.getScript('https://www.google.com/recaptcha/api.js?onload=grecaptchaLoadCallback&render=explicit')
						 .fail(function() { promise.reject(); });
				}

				return promise;
		}
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;
