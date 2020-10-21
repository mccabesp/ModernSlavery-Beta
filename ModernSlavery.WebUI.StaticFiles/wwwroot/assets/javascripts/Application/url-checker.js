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
        //var myheaders = new Headers();
        //var myrequest = new Request(url, {
        //    method: 'get',
        //    mode: 'cors',
        //    credentials: 'omit'
        //});
        //fetch(myrequest)
        //    .then(response => {
        //        debugger;
        //        console.log(url);
        //        console.log(response);
        //    });

        $.ajax({
            url: url,
            dataType: 'jsonp',
            mode: 'cors',
            statusCode: {
                0: function () {
                    console.log($element.data('statement-url') + ": status code 0 returned");
                },
                200: function () {
                    console.log($element.data('statement-url') + ": status code 200 returned");
                },
                404: function () {
                    console.log($element.data('statement-url') + ": status code 404 returned");
                }
            },
            success: function (data, textStatus, jqXHR) {
                console.log($element.data('statement-url') + ": Success");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log($element.data('statement-url') + ": Error");
            },
            complete: function () {
                console.log($element.data('statement-url') + ": Complete");
            }
        });
    };

    function urlSuccess(data, textStatus, jqXHR) {
        debugger;
    };

    function urlFailed(jqXHR, textStatus, errorThrown) {
        debugger;
    };

})(jQuery);