/*global PaymentModal,Utility*/
/// <reference path="references.js" />
/*!
  payment.js
*/
window.Payment = {
    init: function() {
        $("#add-card").click(function() {
            $("#add-card-form").slideToggle();
            return false;
        });
    }
};

window.PaymentModal = {
    modal: [],
    show: function() {
        PaymentModal.modal.modal("show");
    },
    init: function(modal) {
        var thisModal = modal.modal({
            show: false,
            keyboard: false
        });

        PaymentModal.modal = thisModal;

        thisModal.on("show", function() {
            thisModal.find(".page1").show();
            thisModal.find(".page2").hide();
            thisModal.find(".new-card-form").hide();
        });

        thisModal.on("change", "input[name=method]", function() {
            var val = $(this).val();
            thisModal.find(".new-card-form").toggle(val === "[-NEW CARD-]");
            $(this).closest("table").find(".sibling-box input").hide();
            $(this).closest("tr").find("td.sibling-box").find("input").show();
        });

        function showError(err) {
            thisModal.find(".errors").show().empty().append("<li>" + err + "</li>");
        }

        thisModal.find("form").on("submit", function() {
            var form = $(this);
            var data = form.serialize();
            var url = form.attr("action");
            var ajax = $.post(url, data);
            var loader = form.find(".ajax-loader").show();
            var buttons = form.find("button").attr("disabled", "disabled");

            ajax.success(function(resp) {
                if (resp.Errors && resp.Errors.length > 0) {
                    showError(resp.Errors[0].ErrorMessage);
                }
                else {
                    var confirmation = resp;
                    if (confirmation && confirmation.toLowerCase() === "success") {
                        Utility.showOverlay();
                        window.location.reload();
                    }
                    else {
                        thisModal.find(".page1").hide();
                        thisModal.find(".page2").show();
                        thisModal.find(".confirmation-code").text(confirmation);
                    }
                }
            });

            ajax.error(function(resp) {
                showError(resp);
            });

            ajax.always(function() {
                loader.hide();
                buttons.removeAttr("disabled");
            });

            return false;
        });

        thisModal.find(".reload-button").click(function() {
            Utility.showOverlay();
            window.location.reload();
        });

        var cardNumber = thisModal.find("[name=cardnumber]");
        var cvv2 = thisModal.find("[name=cvv2]");
        thisModal.find("[name=provider]").on("change", function() {
            var amexMask = "9999-999999-99999";
            var normalMask = "9999-9999-9999-9999";
            var normalCvvMask = "999";
            var amexCvv = "9999";

            if ($(this).val() === "Amex") {
                cardNumber.unmask().mask(amexMask);
                cvv2.unmask().mask(amexCvv);
            }
            else {
                cardNumber.unmask().mask(normalMask);
                cvv2.unmask().mask(normalCvvMask);
            }
        });
    }
};