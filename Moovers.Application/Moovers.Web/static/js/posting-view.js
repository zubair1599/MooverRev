// <reference path="references.js" />
/*global PaymentModal*/
/*!
    posting-view.js
*/
window.PostingView = {
    init: function() {
        var modal = $("#add-payment-modal");
        PaymentModal.init(modal);
        $(document).on("click", "#add-payment", function() {
            PaymentModal.show();
            return false;
        });
    }
};