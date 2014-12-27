$(function() {
    var elements = $("input[data-errormessage]");
    elements.each(function() {
        var msg = $(this).data("errormessage");
        $(this)[0].oninvalid = function(e) {
            e.target.setCustomValidity("");
            if (!e.target.validity.valid) {
                e.target.setCustomValidity(msg);
            }
        };

        $(this)[0].oninput = function(e) {
            e.target.setCustomValidity("");
        };
    });
})