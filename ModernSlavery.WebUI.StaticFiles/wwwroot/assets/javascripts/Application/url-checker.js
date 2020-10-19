// ping statement url to check it works
(function($) {
    "use strict";
    
    $('.').each(function (i, e) {
    });

    function initialiseUrlCheck($element) {
        var url = $element.data('statement-url');
        $.ajax(url, 
        {
            success: urlSuccess
        });
    };

    function urlSuccess(data, textStatus, jqXHR) {
    };

    function urlFailed(jqXHR, textStatus, errorThrown) {
    };

})(jQuery);