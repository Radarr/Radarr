var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
require('jquery-ui');
var FormatHelpers = require('../../../Shared/FormatHelpers');

var view = Marionette.ItemView.extend({
		template  : 'Settings/Quality/Definition/QualityDefinitionItemViewTemplate',
		className : 'row',

		slider    : {
				min  : 0,
				max  : 400,
				step : 0.1
		},

		ui : {
				sizeSlider          : '.x-slider',
				thirtyMinuteMinSize : '.x-min-thirty',
				sixtyMinuteMinSize  : '.x-min-sixty',
				thirtyMinuteMaxSize : '.x-max-thirty',
				sixtyMinuteMaxSize  : '.x-max-sixty'
		},

		events : {
				'slide .x-slider' : '_updateSize',
                'blur .x-max-thirty' : '_changeMaxThirty'
		},

		initialize : function(options) {
				this.profileCollection = options.profiles;
		},

		onRender : function() {
				if (this.model.get('quality').id === 0) {
						this.$el.addClass('row advanced-setting');
				}

				this.ui.sizeSlider.slider({
						range  : true,
						min    : this.slider.min,
						max    : this.slider.max,
						step   : this.slider.step,
						values : [
								this.model.get('minSize') || this.slider.min,
								this.model.get('maxSize') || this.slider.max
						]
				});

				this._changeSize();
		},

		_updateSize : function(event, ui) {
				var minSize = ui.values[0];
				var maxSize = ui.values[1];

				if (maxSize === this.slider.max) {
						maxSize = null;
				}

				this.model.set('minSize', minSize);
				this.model.set('maxSize', maxSize);

				this._changeSize();
		},

		_changeSize : function() {
				var minSize = this.model.get('minSize') || this.slider.min;
				var maxSize = this.model.get('maxSize') || null;
				{
						var minBytes = minSize * 1024 * 1024;
						var minThirty = FormatHelpers.bytes(minBytes * 90, 2);
						var minSixty = FormatHelpers.bytes(minBytes * 140, 2);

						this.ui.thirtyMinuteMinSize.html(minThirty);
						this.ui.sixtyMinuteMinSize.html(minSixty);
				}

				{
						if (maxSize === 0 || maxSize === null) {
								this.ui.thirtyMinuteMaxSize.val('Unlimited');
								this.ui.sixtyMinuteMaxSize.html('Unlimited');
						} else {
								var maxBytes = maxSize * 1024 * 1024;
								var maxThirty = FormatHelpers.bytes(maxBytes * 90, 2);
								var maxSixty = FormatHelpers.bytes(maxBytes * 140, 2);

								this.ui.thirtyMinuteMaxSize.val(maxThirty);
								this.ui.sixtyMinuteMaxSize.html(maxSixty);
						}
				}
		},

        _changeMaxThirty : function() {
		        var input = this.ui.thirtyMinuteMaxSize.val();
		        var maxSize = parseFloat(input) || 0;
		        var mbPerMinute = maxSize / 90 * 1024;
		        if (mbPerMinute === 0)
                {
                    mbPerMinute = null;
                }
		        this.model.set("maxSize", mbPerMinute);
		        var values = this.ui.sizeSlider.slider("option", "values");
		        values[1] = mbPerMinute || this.slider.max;
		        this.ui.sizeSlider.slider("option", "values", values);
		        this._changeSize();
        }
});

view = AsModelBoundView.call(view);

module.exports = view;
