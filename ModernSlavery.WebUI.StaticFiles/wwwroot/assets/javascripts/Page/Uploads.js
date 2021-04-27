$(document).ready(function () {
    $('[name=files]').change(function () {
        $(this).prevAll(':submit').click();
    });
    
    $("[name=aUpload]").click(function () {
        $(this).prevAll(':file').click();
    });

    $("#btnRecheck").click(function () {
        if (!confirm('Are you sure you want to recheck all companies without SIC codes from companies house?')) return false;
    });
});