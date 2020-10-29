// ping statement url to check it works
(function ($) {
    "use strict";

    $('.app-statement-summary_url')
        .each(function (i, e) {
            initialiseUrlCheck($(e));
        });

    function initialiseUrlCheck($element) {
        var url = $element.data('statement-url');
        console.log(url);

        $.ajax({
            url: url,
            dataType: 'jsonp',
            mode: 'cors',
            statusCode: {
                0: function () {
                    console.log(url + ': 0');
                    urlFailed($element);
                },
                200: function () {
                    console.log(url + ': 200');
                    urlSuccess($element);
                },
                404: function () {
                    console.log(url + ': 404');
                    urlFailed($element);
                }
            },
            timeout: 20000,
            crossDomain: true,
            processData: false,
            complete: function (jqXHR, textStatus, errorThrown) {
                console.log(url + ': complete');
            }
        });
    };

    function urlSuccess($element) {
        $('.app-statement-summary_url-link-working', $element)
            .show();
    };

    function urlFailed($element) {
        $('.app-statement-summary_url-link-broken', $element)
            .show();
    };

})(jQuery);