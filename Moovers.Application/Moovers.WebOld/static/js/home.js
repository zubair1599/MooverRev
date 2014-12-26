/// <reference path="references.js" />
/*global Utility*/
/*!
  home.js
*/
window.HomePage = {
    init: function() {
        $(".panel-section").each(function() {
            var txt = Utility.htmlEncode($(this).text());
            $(this).html(Utility.addCustomLinks(txt));
        });
    }
};