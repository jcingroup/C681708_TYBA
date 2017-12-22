$(function () {

    $(".datepicker").datepicker($.datepicker.regional["zh-TW"]);
    $(".datepicker").datepicker({ dateFormat: 'yy-mm-dd', });
    $(document).ajaxError(function (e, jqxhr, settings, exception) {
        e.stopPropagation();
        if (jqxhr != null) {
            var statusCode = jqxhr.status;
            var response = $.parseJSON(jqxhr.responseText);
            switch (statusCode) {
                case 403:
                    window.location = response.LogOnUrl;
                    break;
                    //case 500:
                    //    alert(response.Message);
                    //    break;
                    //default:
                    //    alert('error');
                    //    break;
            }
        }
    });


});