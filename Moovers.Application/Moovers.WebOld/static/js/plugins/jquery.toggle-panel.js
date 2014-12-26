/// <reference path="references.js" />
(function($) {
    var DEFAULT_OPTS = {
        animate: true
    };

    $.fn.togglePanel = function(options) {
        options = $.extend({}, DEFAULT_OPTS, options);

        $(this).each(function(i) {
            var panel = $(this);
            var hidable = panel.find(".panel-title").siblings();
            var button = panel.find(".icon-toggle");
            var cookieName = panel.data("cookie-name");

            function isHidden() {
                return _.isUndefined($.cookie(cookieName)) ? false : Utility.parseBool($.cookie(cookieName));
            }

            if (isHidden()) {
                hidable.hide();
                button.toggleClass("icon-chevron-down");
            }

            function setHidden(bit) {
                $.cookie(cookieName, bit, { path: "/" });
            }

            var isHidden = !hidable.is(":visible");

            panel.find(".panel-title").click(function() {
                var isHidden = !hidable.is(":visible");
                button.toggleClass("icon-chevron-down", isHidden);

                setHidden(!isHidden);

                if (isHidden) {
                    if (options.animate) {
                        hidable.slideDown();
                    } else {
                        hidable.show();
                    }
                }
                else {
                    if (options.animate) {
                        hidable.slideUp();
                    }
                    else {
                        hidable.hide();
                    }
                }
            });
        });
    };
})(jQuery);