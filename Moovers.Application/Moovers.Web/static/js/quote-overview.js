/// <reference path="references.js" />
/*global PaymentModal */
/*!
    quote-overview.js
*/
window.QuoteOverview = {
    init: function() {
        PaymentModal.init($("#add-payment-modal"));
        $("#add-payment").click(function() {
            PaymentModal.show();
            return false;
        });
    }
};