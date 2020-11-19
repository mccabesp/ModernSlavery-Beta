(function ($) {
    "use strict";

    $('input[type=checkbox][data-mutually-exclusive-group]')
        .each(function (i, e) {
            addChangeHandler($(e))
        });

    function addChangeHandler(element) {
        element.change(function () {

            var isChecked = element.is(':checked');

            // only do something if checking
            if (!isChecked)
                return;

            var name = element.attr('name');
            var group = element.data('mutually-exclusive-group');

            // find other checked checkboxes sharing the same name
            var checkedOthers = $('[name=' + name + ']:checked')
                // not in the same group
                .not('[data-mutually-exclusive-group=' + group + ']');

            // uncheck them
            checkedOthers.prop('checked', false);
        });
    }

}(jQuery));