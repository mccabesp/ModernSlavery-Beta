// ping statement url to check it works
(function ($, window) {
    "use strict";

    $('.app-statement-summary_url')
        .each(function (i, e) {
            initialiseUrlCheck($(e));
        });

    function initialiseUrlCheck($element) {
        var url = $element.data('statement-url');

        $.ajax({
            url: url,
            dataType: 'jsonp',
            timeout: 20000,
            statusCode: {
                0: function () {
                    // timed out and did not give 200
                    urlFailed($element);
                },
                200: function () {
                    urlSuccess($element);
                }
            },
            crossDomain: true,
            processData: false,
        });
    };

    function urlSuccess($element) {
        $('.app-statement-summary_url-link-broken', $element)
            .hide();
        $('.app-statement-summary_url-link-working', $element)
            .show();
    };

    function urlFailed($element) {
        $('.app-statement-summary_url-link-working', $element)
            .hide();
        $('.app-statement-summary_url-link-broken', $element)
            .show();
    };

})(jQuery, window);