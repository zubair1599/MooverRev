/*global Pricing,Utility,PRICING_VARS,Quotes*/
/// <reference path="references.js" />
/*!
    pricing.js
*/
(function (exports) {
    exports.Pricing = {
        templates: {},
        type: "Binding",

        dialogs: {
            changePricing: '#change-pricing-modal'
        },

        init: function () {
            Utility.initBase(exports.Pricing);

            Pricing.couponApplied = PRICING_VARS.DISCOUNTCOUPONUSED;
            Pricing.couponCode = PRICING_VARS.DISCOUNTCOUPONCODE;
            $("#discountCouponVoucher").val(Pricing.couponCode);
            if (Pricing.couponApplied) {
                $("#discountCouponVoucher").prop('readonly', true);
            }

            $("#change-pricing").click(function () {
                Pricing.dialogs.changePricing.modal("show");
                return false;
            });

            Pricing.type = PRICING_VARS.TYPE;
            var sourceTime = PRICING_VARS.ESTIMATED_SOURCE_TIME;
            var travelTime = PRICING_VARS.ESTIMATED_TRAVEL_TIME;

            var activeIndex = (Pricing.type === "Hourly") ? 1 : 0;
            $("#price-tabs").tabs({
                activate: function (event, ui) {
                    var firstInput = ui.newPanel.find("input,select").first();
                    firstInput.show().focus();
                    if (firstInput.is(".ui-disable-stupid")) {
                        firstInput.hide();
                    }

                    if (ui.newPanel.is("#price-guaranteed")) {
                        Pricing.type = "Binding";
                    } else if (ui.newPanel.is("#price-hourly")) {
                        Pricing.type = "Hourly";
                    }

                    Pricing.applyAdjustment();
                    Pricing.calculateHourly();
                    Pricing.calculateTotalCost();

                },
                active: activeIndex
            });

            if (sourceTime > SERVER.MAX_HOURLY_SOURCE_TIME || travelTime > SERVER.MAX_HOURLY_TRAVEL_TIME) {
                $("#price-tabs").tabs("option", "disabled", [1]);
            }

            $("#adjustment").bind("blur", function () {


                $.post("/Quote/DiscountPriority", { id: $("#lookupID").text(), discountType: "percent", percent: $("#adjustment").val() }, null, "json");

                Pricing.applyPercentageAdjustment();
                $("#valuationTypeGuaran").focus();
                //Pricing.applyAdjustment();
            });


            $("[name=valuationTypeGuaranteed]").change(function () {
                Pricing.applyAdjustment();
            });

            $("[name=valuationTypeHourly]").change(function () {
                Pricing.calculateHourly();
            });

            $("#adjustment-amount").bind("blur", function () {
                $.post("/Quote/DiscountPriority", { id: $("#lookupID").text(), discountType: "value", percent: "" }, function (data) {

                }, "json");
                Pricing.applyAdjustment();
                $("#valuationTypeGuaran").focus();
            });

            $("#Trucks").change(function () {
                var trucks = $(this).val();
                var selected = $("#CrewSize").val();
                $("#CrewSize").empty().append(_.map(Pricing.getValidCrewSizes(trucks), function (i) {
                    return "<option value='" + i + "'>" + i + "</option>";
                }));

                if (selected) {
                    $("#CrewSize").val(selected);
                }

                if (!$("#CrewSize").val()) {
                    var firstOpt = $("#CrewSize option:first").val();
                    $("#CrewSize").val(firstOpt);
                }
            });

            $("#Trucks-Hourly").change(function () {
                var trucks = $(this).val();
                var selected = $("#CrewSize-Hourly").val();
                $("#CrewSize-Hourly").empty().append(_.map(Pricing.getValidCrewSizes(trucks), function (i) {
                    return "<option value='" + i + "'>" + i + "</option>";
                }));

                $("#CrewSize-Hourly").val(selected);

            });
            $("#Trucks-Hourly").change();

            $("#Trucks,#CrewSize,#Hours,#additionalDestination").change(function () {
                Pricing.fillValuation(Pricing.calculateHourly);
            });

            $("#btn-next,#btn-back").click(function () {
                var url = $(this).attr("href");
                Pricing.save(function () {
                    window.location = url;
                });
                return false;
            });

            $(".pricing-miscellanous").click(function () {
                $("#miscellaneous-pricing").show();
                $(this).hide();
                return false;
            });

            $("#edit-storage-button").click(function () {
                $("#edit-storage").toggle();
            });

            $("[name=packingMaterials],[name=temporaryStorage],[name=replacementValuationsGuaranteed],[name=replacementValuationsHourly]").change(function () {
                Pricing.calculateTotalCost();
            });

            Pricing.fillValuation(function () {
                Pricing.calculateHourly();
                Pricing.applyAdjustment();
                Pricing.calculateTotalCost();
            });

            $("#Trucks").change();

            var scheduleUrl = SERVER.baseUrl + "Quote/Schedule/" + Quotes.lookup;

            var save = function () {
                Utility.showOverlay();
                Pricing.save(function () {
                    window.location = scheduleUrl;
                });

                return false;
            };

            $("#miscellaneous-pricing-form").submit(save);
            $(".quote-save-button").click(save);

            $("#discountCouponVoucher").bind("blur", function () {
                Pricing.validateCoupon();
            });
        },

        showValidCoupon: function () {
            $("#discountCouponVoucherOk").show();
            $("#discountCouponValidMessage").show();
        },

        showInValidCoupon: function () {
            $("#discountCouponVoucherInvalid").show();
            $("#discountCouponInValidMessage").show();
        },

        showCouponAlreadyApplied: function () {
            $("#discountCouponAlreadyAppliedMessage").show();
        },

        hideAllCouponValidation: function () {
            $("#discountCouponVoucherOk").hide();
            $("#discountCouponVoucherInvalid").hide();
            $("#discountCouponValidMessage").hide();
            $("#discountCouponInValidMessage").hide();
            $("#discountCouponAlreadyAppliedMessage").hide();
        },

        applyDiscountCoupon: function (data) {
            if (data) {
                if (data.valueType == 1) {
                    var percent = parseFloat($("#adjustment").val());
                    percent = Math.abs(percent);
                    percent = percent + parseFloat(data.value);
                    $("#adjustment").val(percent * -1);
                    Pricing.discountCoupon = data;
                    Pricing.applyPercentageAdjustment();
                } else if (data.valueType == 2) {
                    var amount = parseFloat($("#adjustment-amount").val());
                    amount = Math.abs(amount);
                    amount = amount + parseFloat(data.value);
                    $("#adjustment-amount").val(amount * -1);
                    Pricing.discountCoupon = data;
                    Pricing.applyAdjustment();
                }

                Pricing.couponApplied = true;
                Pricing.couponCode = $("#discountCouponVoucher").val();
            }
        },

        removeDiscountCoupon: function () {
            var data = Pricing.discountCoupon;
            if (data) {
                if (data.valueType == 1) {
                    var percent = parseFloat($("#adjustment").val());
                    percent = Math.abs(percent);
                    percent = percent - parseFloat(data.value);
                    $("#adjustment").val(percent * -1);
                    Pricing.discountCoupon = data;
                    Pricing.applyPercentageAdjustment();
                } else if (data.valueType == 2) {
                    var amount = parseFloat($("#adjustment-amount").val());
                    amount = Math.abs(amount);
                    amount = amount - parseFloat(data.value);
                    $("#adjustment-amount").val(amount * -1);
                    Pricing.discountCoupon = data;
                    Pricing.applyAdjustment();
                }

                Pricing.couponApplied = false;
                Pricing.couponCode = null;
            }
        },

        validateCoupon: function () {
            var code = $("#discountCouponVoucher").val();

            if (!code || code == "") {
                Pricing.hideAllCouponValidation();
                Pricing.removeDiscountCoupon();
                return;
            }

            if (Pricing.couponApplied) {
                Pricing.showCouponAlreadyApplied();
                $("#discountCouponVoucher").val(Pricing.couponCode);
                return;
            }

            $.ajax({
                url: '/Quote/ValidateCoupon',
                type: 'GET',
                dataType: 'json',
                data: { code: code },
                cache: false,
                success: function (data, textStatus, jqXHR) {

                    Pricing.hideAllCouponValidation();

                    if (data && data.isValid == true) {

                        Pricing.showValidCoupon();
                        Pricing.applyDiscountCoupon(data);

                    } else if (data.isValid == false) {

                        Pricing.showInValidCoupon();

                    } else {
                        Pricing.showInValidCoupon();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                }
            });
        },

        // adjustment = percentage (i.e. 4 = 4%), returns adjustment amount rounded to nearest cent
        calculateAdjustment: function (base, adjustment) {
            return Math.floor((base * (adjustment / 100)) * 100) / 100;
        },

        // returns a list of valid Crew Sizes for integer truck #
        getValidCrewSizes: function (numTrucks) {
            var min = numTrucks * 2;
            var max = numTrucks * 3;
            return Utility.range(min, max, 1);
        },
        calculateTotalCost: function () {
            var moveCost = (Pricing.type === "Hourly") ? Utility.parseCurrency($("#hourly-total").text()) : Utility.parseCurrency($("#total").text());
            var packingMaterials = parseFloat($("[name=packingMaterials]").val()) || 0;
            var storageCost = this.calculateStorageCost();
            var total = moveCost + packingMaterials + storageCost;
            $("#total-move-price").text(Utility.formatCurrency(total));
        },

        calculateStorageCost: function () {
            var temp = $("#temporary-storage");
            var monthly = $("#monthly-storage");

            var cost = 0;
            if (temp.length > 0) {
                cost += Utility.parseCurrency($.trim(temp.text()));
            }
            if (monthly.length > 0) {
                cost += Utility.parseCurrency($.trim(monthly.text()));
            }

            return cost;
        },

        fillValuation: function (callback) {
            var quoteid = PRICING_VARS.QUOTEID;
            var trucks = parseInt($("#Trucks").val(), 10) || 0;
            var people = parseInt($("#CrewSize").val(), 10) || 0;
            var estimatedHours = parseInt($("#Hours").val(), 10) || 0;

            $.ajax({
                url: '/Quote/GetHourlyValuations',
                type: 'GET',
                dataType: 'html',
                data: { quoteid: quoteid, numTrucks: trucks, crewSize: people, estimateTime: estimatedHours },
                // we set cache: false because GET requests are often cached by browsers
                // IE is particularly aggressive in that respect
                cache: false,

                success: function (data) {
                    $('#replacementValuationsHourly').html(data);
                    callback();
                }
            });
        },

        // Calculates the hourly rate and updates various UI elements
        calculateHourly: function () {
            if (Pricing.type === "Hourly") {
                var personPrice = PRICING_VARS.PERSON_PRICE_MULTIPLIER;
                var truckPrice = PRICING_VARS.TRUCK_PRICE_MULTIPLIER;
                var personPriceDestination = PRICING_VARS.PERSON_DESTINATION_MULTIPLIER;
                var truckPriceDestination = PRICING_VARS.TRUCK_DESTINATION_MULTIPLIER;
                var currentHour = PRICING_VARS.CURRENT_HOUR;

                var trucks = parseInt($("#Trucks").val(), 10) || 0;
                var people = parseInt($("#CrewSize").val(), 10) || 0;
                var estimatedHours = parseInt($("#Hours").val(), 10) || 0;

                var perHour = (personPrice * people) + (truckPrice * trucks);
                var firstHour = (personPriceDestination * people) + (trucks * truckPriceDestination) + perHour;

                var additionalDestination = Utility.parseCurrency($("#additionalDestination").val());
                if (additionalDestination) {
                    firstHour += additionalDestination;
                }

                var valuation = parseFloat($("[name=valuationTypeHourly] :selected").data("cost"));

                var cost = ((estimatedHours - 1) * perHour) + firstHour + valuation;

                $("#estimated-hourly-price").text(Utility.formatCurrency(cost));
                $("#hourly-total").text(Utility.formatCurrency(cost));

                $("#first-hour").text(Utility.formatCurrency(firstHour));
                $("#first-hour-lineprice").text(Utility.formatCurrency(firstHour) + " first hour");

                $("#extra-hours").text(Utility.formatCurrency(perHour));
                $("#hourly-lineprice").text(Utility.formatCurrency(perHour) + " /hour");

                $("#hourly-pricing-summary").show();
                $("#guaranteed-pricing-summary").hide();
            }
        },

        applyPercentageAdjustment: function () {
            var adjustmentBox = $("#adjustment");
            var amountBox = $("#adjustment-amount");
            var base = parseFloat($("#base-price").text().replace(/[^-\d.]/g, ""));
            var percent = Utility.roundTo(parseFloat(adjustmentBox.val().replace(/[^-\d.]/g, "")), 2);
            var adjustmentAmount = Pricing.calculateAdjustment(base, percent);

            $.post("/Quote/GetDiscountPriority", { id: $("#lookupID").text() }, function (data) {
                if (data == "0") {
                }
                else if (data == "1") {
                    $.post("/Quote/DiscountPercentage", { id: $("#lookupID").text() }, function (data2) {
                        percent = Utility.roundTo(parseFloat(data2.replace(/[^-\d.]/g, "")), 2);
                        adjustmentAmount = Pricing.calculateAdjustment(base, percent);
                    }, "json");
                }
            }, "json");
            adjustmentBox.val(percent);
            amountBox.val(adjustmentAmount);
            Pricing.applyAdjustment();
        },

        // Calculates adjustments and updates various UI elements
        applyAdjustment: function () {
            if (Pricing.type === "Binding") {
                var adjustmentBox = $("#adjustment-amount");
                var adjustmentAmount = Utility.roundTo(parseFloat(adjustmentBox.val()), 2);
                var base = parseFloat($("#base-price").text().replace(/[^\d.]/g, ""));
                var adjustmentPercent = Utility.roundTo((adjustmentAmount / base) * 100, 2);
                var valuation = parseFloat($("[name=valuationTypeGuaranteed] :selected").data("cost"));
                var done = false;
                $.post("/Quote/GetDiscountPriority", { id: $("#lookupID").text() }, function (data) {
                    if (data == "0") {
                        base = valuation + base;
                        done = true;
                        if (base === 0) {
                            adjustmentAmount = 0;
                            $("#adjustment-amount").removeClass("error");
                        }
                        else if (adjustmentPercent > SERVER.MAX_PRICE_DISCOUNT || adjustmentPercent < -SERVER.MAX_PRICE_DISCOUNT || isNaN(adjustmentPercent) || isNaN(adjustmentAmount)) {
                            $("#adjustment-amount").addClass("error");
                            adjustmentAmount = 0;
                        }
                        else {
                            $("#adjustment-amount").removeClass("error");
                        }
                        $("#adjustment-amount").val(adjustmentAmount);
                        $("#adjustment").val(adjustmentPercent);
                        $("#total").text(Utility.formatCurrency(Utility.roundTo(base + adjustmentAmount, 2)));
                        $("#hourly-pricing-summary").hide();
                        $("#guaranteed-pricing-summary").show();
                        Pricing.calculateTotalCost();
                    }
                    else if (data == "1") {
                        done = true;
                        $.post("/Quote/DiscountPercentage", { id: $("#lookupID").text() }, function (data2) {
                            adjustmentPercent = Utility.roundTo(parseFloat(data2), 2);
                            adjustmentAmount = Pricing.calculateAdjustment(base, adjustmentPercent);

                            base = valuation + base;

                            if (base === 0) {
                                adjustmentAmount = 0;
                                $("#adjustment-amount").removeClass("error");
                            }
                            else if (adjustmentPercent > SERVER.MAX_PRICE_DISCOUNT || adjustmentPercent < -SERVER.MAX_PRICE_DISCOUNT || isNaN(adjustmentPercent) || isNaN(adjustmentAmount)) {
                                $("#adjustment-amount").addClass("error");
                                adjustmentAmount = 0;
                            }
                            else {
                                $("#adjustment-amount").removeClass("error");
                            }
                            $("#adjustment-amount").val(adjustmentAmount);
                            $("#adjustment").val(adjustmentPercent);
                            $("#total").text(Utility.formatCurrency(Utility.roundTo(base + adjustmentAmount, 2)));
                            $("#hourly-pricing-summary").hide();
                            $("#guaranteed-pricing-summary").show();
                            Pricing.calculateTotalCost();

                        }, "json");
                    }
                    
                }, "json");

                if (!done) {
                    base = valuation + base;
                    done = true;
                    if (base === 0) {
                        adjustmentAmount = 0;
                        $("#adjustment-amount").removeClass("error");
                    }
                    else if (adjustmentPercent > SERVER.MAX_PRICE_DISCOUNT || adjustmentPercent < -SERVER.MAX_PRICE_DISCOUNT || isNaN(adjustmentPercent) || isNaN(adjustmentAmount)) {
                        $("#adjustment-amount").addClass("error");
                        adjustmentAmount = 0;
                    }
                    else {
                        $("#adjustment-amount").removeClass("error");
                    }
                    $("#adjustment-amount").val(adjustmentAmount);
                    $("#adjustment").val(adjustmentPercent);
                    $("#total").text(Utility.formatCurrency(Utility.roundTo(base + adjustmentAmount, 2)));
                    $("#hourly-pricing-summary").hide();
                    $("#guaranteed-pricing-summary").show();
                    Pricing.calculateTotalCost();
                }
            }
        },
        save: function (cb) {
            cb = cb || $.noop;
            var adjustmentBox = $("#adjustment-amount");
            var adjustmentAmount = (adjustmentBox.val() !== "") ? parseFloat(adjustmentBox.val()) : 0;

            var combinedObj = {
                packingMaterials: $("[name=packingMaterials]").val(),
                temporaryStorage: $("[name=temporaryStorage]").val(),
                forcedStorage: $("[name=force-storage-cost]").val()
            };

            var loaders = $("#guaranteed-loader,#hourly-loader").show();
            if (Pricing.type === "Binding") {
                var trucks = parseInt($("#Trucks-Hourly").val(), 10);
                var crew = parseInt($("#CrewSize-Hourly").val(), 10);
                var bindingobj = {
                    quoteid: Quotes.quoteid,
                    adjustment: adjustmentAmount,
                    trucks: trucks,
                    crewsize: crew,
                    valuationType: $("[name=valuationTypeGuaranteed]").val(),
                    discountCouponCode: Pricing.couponCode
                };

                var bindingpost = $.post(SERVER.baseUrl + "Quote/SaveGuaranteed", $.extend(combinedObj, bindingobj));
                bindingpost.success(function () {
                    cb();
                });
                bindingpost.always(function () {
                    loaders.hide();
                });
            }
            else if (Pricing.type === "Hourly") {
                var hourlyPrice = parseFloat($("#base-price").text().replace(/[^\d.]/g, ""));
                var hourlyobj = {
                    quoteid: Quotes.quoteid,
                    numTrucks: $("#Trucks").val(),
                    crewSize: $("#CrewSize").val(),
                    estimateTime: $("#Hours").val(),
                    additionalDestination: $("#additionalDestination").val(),
                    hourlyPrice: hourlyPrice,
                    valuationType: $("[name=valuationTypeHourly]").val()
                };

                var hourlypost = $.post(SERVER.baseUrl + "Quote/SaveHourly", $.extend(combinedObj, hourlyobj));
                hourlypost.success(function () {
                    cb();
                });

                hourlypost.always(function () {
                    loaders.hide();
                    Utility.hideOverlay();
                });
            }
        }
    };
})(window);