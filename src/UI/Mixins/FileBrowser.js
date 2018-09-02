var $ = require('jquery');
var vent = require('vent');
require('../Shared/FileBrowser/FileBrowserLayout');
require('./DirectoryAutoComplete');

$.fn.fileBrowser = function(options) {
    var inputs = $(this);

    inputs.each(function() {
        var input = $(this);
        var inputOptions = $.extend({ input : input, showFiles: input.hasClass('x-filepath') }, options);
        var inputGroup = $('<div class="input-group"></div>');
        var inputGroupButton = $('<span class="input-group-btn"></span>');

        var button = $('<button class="btn btn-primary x-file-browser" title="Browse"><i class="icon-radarr-folder-open"/></button>');

        if (input.parent('.input-group').length > 0) {
            input.parent('.input-group').find('.input-group-btn').prepend(button);
        } else {
            inputGroupButton.append(button);
            input.wrap(inputGroup);
            input.after(inputGroupButton);
        }

        button.on('click', function() {
            vent.trigger(vent.Commands.ShowFileBrowser, inputOptions);
        });

        input.directoryAutoComplete({ includeFiles: inputOptions.showFiles });
    });

};
