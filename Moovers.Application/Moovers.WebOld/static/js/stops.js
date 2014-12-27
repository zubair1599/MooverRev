/// <reference path="references.js" />
/*!
    stops.js
*/
/*global _,$,Utility,initVerifyAddress,Stop,Stops,Quotes,Address,google*/
(function(exports) {
    var StopModal = {
        // Array of Stops
        currentStops: [],

        // Current Select Stop
        currentStopIndex: 0,

        // Address search results for  "stop"
        // { stopid: [ addressresults ] }
        searchResults: { },

        currentPage: null,

        initNew: function() {
            StopModal.currentStops.empty();
        },
        initEdit: function() {
            StopModal.resetModal();
            Stops.elements.stopModal.find("#btn-delete-stop").show();
            Stops.elements.stopModal.find("button[data-continue=true]").hide();
            StopModal.showPage("page1");
        },

        isFirst: function() {
            return StopModal.currentStopIndex === 0;
        },
        isLast: function() {
            return StopModal.currentStopIndex === (StopModal.currentStops.length - 1);
        },
        // Shows an already added stop
        showStop: function(stop) {
            // editing a stop from the page
            if (!StopModal.currentStops || StopModal.currentStops.length === 0) {
                StopModal.stopToForm(stop);
                StopModal.showPage("page1");
                Stops.elements.stopModal.find("#selected-address").show();
                return;
            }

            StopModal.currentStop = stop;
            if (StopModal.isFirst()) {
                Stops.elements.stopModal.find(".asdofsadfsadf").toggleClass("");
            }
            if (StopModal.isLast()) {
                Stops.elements.stopModal.find(".asdofsadfsadf").toggleClass("");
            }
        },
        actions: {
            close: function() {
                StopModal.resetModal();
                $(".modal.in").modal("hide");
            },
            forward: function() {
                if (StopModal.currentPage === "page1") {
                    StopModal.showPage("page2");
                }
                else if (!StopModal.isLast()) {
                    StopModal.showStop(StopModal.currentStops[StopModal.currentStopIndex + 1]);
                }
            },
            back: function() {
                if (StopModal.currentPage === "page2") {
                    StopModal.showPage("page1");
                }
                else if (!StopModal.isFirst()) {
                    StopModal.showStop(StopModal.currentStops[StopModal.currentStopIndex - 1]);
                }
            },
            save: function() {
                var form = Stops.elements.stopModal.find("form.page2");
                var cont = $(this).data("continue");
                form.find("[type=submit]").data("continue", cont).click();
                return false;
            }
        },
        resetModal: function() {
            Stops.elements.stopModal.find(".search-results").hide();
            Stops.elements.stopModal.find(".no-search-results").show();
            Stops.elements.stopModal.find("#searchTextField").val("");
            Stops.elements.stopModal.find("#selected-address").hide().find("[checked]").removeAttr("checked");
            Stops.elements.stopModal.find(".verified-address-container").empty();
            Stops.elements.stopModal.find(".unverified-address-container").empty();
            Stops.elements.stopModal.find(".show-custom").removeClass(".show-custom");
            Stops.elements.stopModal.find("form").each(function() {
                this.reset();
            });
        },
        templateHeader: function(idx) {
            var container = Stops.elements.stopModal.find(".nav-ul-steps");
            var data = Utility.range(0, idx).map(function(r) {
                return Stops.templates.navTemplate({ Index : r });
            }).join("");
            container.empty().append(data);
        },
        showPage: function(page) {
            if (page === "page2") {
                var verifiedAddress = $("input[name='address-select']:checked");
                if (verifiedAddress.length === 0) {
                    showError("page1", "Please select an address");
                    return false;
                }
            }

            $("#address-find-error").hide();
            StopModal.currentPage = page;
            StopModal.templateHeader(StopModal.currentStops.length);
            Stops.elements.stopModal.find(".page").hide();
            Stops.elements.stopModal.find(".nav-ul-steps").find("li").removeClass("active");
            Stops.elements.stopModal.find(".add-stop-nav-btns").hide();

            var lis = Stops.elements.stopModal.find(".nav-ul-steps li");
            var els = lis.eq(StopModal.currentStopIndex * 2).add(lis.eq((StopModal.currentStopIndex * 2) + 1));
            var form = Stops.elements.stopModal.find("." + page);
            form.trigger("show");
            form.show();
            $(els).filter("[data-step=" + page + "]").addClass("active");
        },
        stopToForm: function(stop) {
            stop = stop || {};
            var modal = Stops.elements.stopModal;
            _.each(stop, function(val, key) {
                var input = modal.find("[name=" + key + "]");
                if (!input.is("[type=radio],[type=submit],[type=checkbox]")) {
                    input.val(val);
                }
                else {
                    input.toggleAttr("checked", val);
                }
            });

            modal.find("[name=stopid]").val(stop.id);
            modal.find(".search-results").hide();
            modal.find("#selected-display").text(stop.address.display());
            modal.find("#selected-address").show().find("input[type=radio]").attr("checked", "checked");
        }
    };

    exports.Stops = {
        franchiseAddress: [],
        templates: {
            distance: "#distance-template",
            stop: "#stop-template",
            navTemplate: "#stops-nav-template"
        },
        elements: {
            error: "#address-find-error",
            stopModal: "#add-stop-modal",
            storageModal: "#add-storage-modal",
            stopLoader: "#save-stop-loader",
            save: "#add-stop-modal .save",
            modalPage1: "#add-stop-modal .page1",
            modalPage2: "#add-stop-modal .page2"
        },
        stops: [],
        getMapUrl: function(addr) {
            if (addr.Street1) {
                var uri = encodeURI(addr.display().replace(/[#?$+,\/:&;=@%{}|\\\[\]\^~]/g, ""));
                return "http://maps.googleapis.com/maps/api/streetview?size=460x400&location=" + uri + "&sensor=false";
            }

            return false;
        },
        init: function(stops) {
            Utility.initBase(Stops);

            Stops.franchiseAddress = window.FranchiseAddress;

            Stops.stops = _.map(stops, function(s) {
                return new Stop(s);
            });

            Stops.elements.stopModal.modal({
                show: false,
                keyboard: false
            });

            Stops.elements.storageModal.modal({
                show: false,
                keyboard: false
            });

            Stops.elements.storageModal.find("[name=days]").change(function() {
                var val = $(this).val();
                Stops.elements.storageModal.find(".storage-msg").toggle(val === "9999");
            });

            $("#add-stop").click(function() {
                Stops.elements.stopModal.find("#btn-delete-stop").hide();
                Stops.elements.stopModal.modal("show");
                Stops.elements.stopModal.find("[name=stopid]").val("");
                StopModal.showPage("page1");
                StopModal.resetModal();
            });

            $("#add-storage").click(function() {
                Stops.elements.storageModal.modal("show");
            });

            // deleting a stop causes a hard page reload
            $("#btn-delete-stop").click(function() {
                var stopid = $("input[name=stopid]").val();
                if (stopid && typeof (stopid) === "string") {
                    window.location = SERVER.baseUrl + "Quote/DeleteStop/" + stopid;
                }

                return false;
            });

            $("#select-current-address").click(function() {
                var address = new Address($(this).data("address"));
                var form = $(".addr-search");
                address.toForm(form);
                form.submit();
                return false;
            });

            Stops.elements.stopModal.bind("show", function() {
                StopModal.initNew();
            });

            // initialize the "Autocomplete" address field
            Stops.elements.stopModal.one("show", function() {
                var input = $("#searchTextField")[0];
                var options = {
                    componentRestrictions: { country: "us" }
                };

                var autocomplete = new google.maps.places.Autocomplete(input, options);
                google.maps.event.addListener(autocomplete, "place_changed", function() {
                    var place = autocomplete.getPlace();
                    function getVal(name, type) {
                        type = type || "long_name";
                        var ret = _.filter(place.address_components, function(i) {
                            return _.any(i.types, function(r) {
                                return r === name;
                            });
                        })[0];

                        return (ret ? ret[type] : "");
                    }

                    if (place.address_components) {
                        var vals = {
                            street1: getVal("street_number") + " " + getVal("route"),
                            city: getVal("locality"),
                            state: getVal("administrative_area_level_1", "short_name"),
                            zip: getVal("postal_code")
                        };

                        Utility.objectToForm(vals, Stops.elements.stopModal.find("form.addr-search"));
                        if (vals.zip) {
                            Stops.elements.stopModal.find("form.addr-search").find("[type=submit]").click();
                        }
                    } else if (place.name) {
                        Stops.elements.stopModal.find("form.addr-search [name=street1]").val(place.name).focus();

                    }
                });
            });

            // when entering a state, select the state that goes with it if possible
            Stops.elements.stopModal.on("blur", "[name=zip]", function() {
                var stateBox = $(this).closest("form").find("[name=state]");
                if (stateBox.val() === "") {
                    var zip = $(this).val();
                    stateBox.val(Utility.findStateFromZip(zip));
                }
            });

            Stops.elements.stopModal.on("click", "button", function(e) {
                var target = $(e.target);
                if (target.is(".cancel")) {
                    StopModal.actions.close();
                }
                if (target.is(".next")) {
                    StopModal.actions.forward();
                }
                if (target.is(".back")) {
                    StopModal.actions.back();
                }
                if (target.is(".save")) {
                    // the buttons in the modals are outside of the forms that are submitted
                    // fill a hidden field in the form when these buttons are clicked
                    var cont = target.data("continue");
                    Stops.elements.modalPage2.find("input[name=continue]").val(cont);
                    StopModal.actions.save();
                }
            });

            // initialize modal page 1
            Stops.elements.modalPage1.bind("show", function() {
                $("#selected-address").hide();
                $("#searchTextField").val("");
                clearErrors();
                return false;
            });

            initVerifyAddress({
                addressForm: Stops.elements.modalPage1.find("form.addr-search"),
                verifyContainer: Stops.elements.modalPage1.find(".verified-address-container"),
                unverifyContainer: Stops.elements.modalPage1.find(".unverified-address-container"),
                submit: function() {
                    clearErrors();
                    Stops.elements.modalPage1.find(".no-search-results").hide();
                },
                error: function(msg) {
                    showError("page1", msg);
                    Stops.elements.modalPage1.find("form.addr-search").find("[name=zip]").addClass("error");
                },
                success: function() {
                    Stops.elements.modalPage1.find(".search-results").show();
                }
            });

            // initialize modal page 2
            Stops.elements.modalPage2.on("submit", function() {
                var form = $(this);
                var selectedAddress = Stops.elements.modalPage1.find("[name=address-select]:checked");
                var verified = selectedAddress.is(".verified");
                var page1Data = Stops.elements.stopModal.find(".page1 form.addr-search").serializeObject();
                var stopid = Stops.elements.stopModal.find("[name=stopid]").val();
                var current = _.find(Stops.stops, function(s) {
                    return s.id === stopid;
                });

                var data = form.serializeObject();
                data = $.extend({}, data, page1Data);
                data.id = stopid;

                if (selectedAddress.val() === "CURRENT_SELECTED" && current) {
                    var props = ["addressid", "city", "state", "street1", "street2", "verified", "verifiedAddress", "zip"];
                    _.each(props, function(p) {
                        data[p] = current[p];
                    });
                }
                else {
                    data.verified = verified;
                    data.verifiedAddress = selectedAddress.val();
                }

                // if we're editing an item, don't reset its sort.
                if (current) {
                    data.sort = current.sort;
                }
                // set the "sort" to the highest of the current sorts
                if (_.isUndefined(data.sort)) {
                    data.sort = _.max(_.pluck(Stops.stops, "sort")) + 1;
                    if (data.sort < 0) {
                        data.sort = 0;
                    }
                }

                // if editing, remove the stop before we we-add it
                Stops.stops = _.filter(Stops.stops, function(s) {
                    return s.id !== data.id;
                });

                // add stop to memory collection
                var stop = new Stop(data);
                Stops.stops.push(stop);
                Stops.stops = _.sortBy(Stops.stops, function(s) {
                    return s.sort;
                });

                // for "Save and Close", grab a boolean so we can tell whether or not to show an empty modal.
                var cont = Utility.parseBool(data["continue"]);
                Stops.save(function() {
                    Stops.retemplate();
                    Quotes.updateQuicklook();
                    if (cont) {
                        $("#btn-delete-stop").hide();
                        Stops.elements.stopModal.modal("show");
                        Stops.elements.stopModal.find("[name=stopid]").val("");
                        StopModal.showPage("page1");
                        StopModal.resetModal();
                    }
                    else {
                        $(".modal.in").modal("hide");
                    }
                });
                
                return false;
            });

            // "Floor" is disabled unless you select an elevator
            var floorBox = Stops.elements.modalPage2.find("[name=floor]");
            var elevatorBox = Stops.elements.modalPage2.find("#elevatorType");
            Stops.elements.modalPage2.bind("show", function() {
                var address = new Address($("form.addr-search").serializeObject());

                var url = Stops.getMapUrl(address);
                if (url) {
                    $("#maps-holder").attr("src", Stops.getMapUrl(address)).show();
                }
                else {
                    $("#maps-holder").hide();
                }

                floorBox.toggleAttr("disabled", elevatorBox.val() === "No_Elevator");
                floorBox.toggleAttr("required", elevatorBox.val() !== "No_Elevator");
                return false;
            });

            elevatorBox.change(function() {
                floorBox.toggleAttr("disabled", elevatorBox.val() === "No_Elevator");
                floorBox.toggleAttr("required", elevatorBox.val() !== "No_Elevator");
            });

            // edit stop
            $("#stop-container").on("click", ".stop-edit", function() {
                var stopid = $(this).data("stopid");
                var stop = _.find(Stops.stops, function(s) {
                    return s.id === stopid;
                });
                
                StopModal.initEdit();
                Stops.elements.stopModal.modal("show");
                StopModal.showStop(stop);
                return false;
            });

            Stops.elements.stopLoader.hide();
            Stops.retemplate();
        },
        save: function(cb) {
            var json = JSON.stringify(Stops.stops);
            var quoteid = Quotes.quoteid;
            Stops.elements.stopLoader.show();
            Stops.elements.save.addClass("disabled").attr("disabled", "disabled");

            $.postq("Quotes", SERVER.baseUrl + "Quote/Stops", { quoteid: quoteid, stopsjson: json }, function(resp) {
                _.each(resp, function(val, key) {
                    var stop = _.find(Stops.stops, function(s) {
                        return s.id === key;
                    });

                    stop.id = val[0];
                    stop.addressid = val[1];
                    stop.address.AddressID = val[1];
                });

                Stops.elements.stopLoader.hide();
                Stops.elements.save.removeClass("disabled").removeAttr("disabled");
                (cb || $.noop)();
            });
        },
        retemplate: function() {
            var container = $("#stop-container").empty();
            var stops = _.sortBy(Stops.stops, function(s) {
                return s.sort;
            });

            _.each(stops, function(stop) {
                var distanceTemplate = Stops.templates.distance;
                var content = $.trim(distanceTemplate(stop));
                var prev = $($.parseHTML(content));
                var next = $($.parseHTML(content));
                calculateDistances(stop, prev, next);
                container.append(prev).append($.trim(Stops.templates.stop(stop))).append(next);
            });
        }
    };

    function clearErrors() {
        Stops.elements.error.empty().hide();
        Stops.elements.stopModal.find("input,select").removeClass("error");
    }

    function showError(page, msg) {
        Stops.elements.error.empty().text(msg).show();
    }

    // a separate request is made for calculating distances between each stops
    function calculateDistances(stop, prevContainer, nextContainer) {
        var idx = Stops.stops.indexOf(stop);
        var prev = (idx === 0) ? Stops.franchiseAddress : Stops.stops[idx - 1].address;
        var next = (idx === Stops.stops.length - 1) ? Stops.franchiseAddress : Stops.stops[idx + 1].address;

        if (idx === 0) {
            getDistanceFrom(prev, stop.address, function(resp) {
                prevContainer.find(".loading").hide();
                prevContainer.find(".loaded").show();
                prevContainer.find(".mileage").text(resp.distance);
                prevContainer.find(".hours").text(Utility.formatHours(resp.time));
            });
        } else {
            prevContainer.hide().find(".loading").remove();
            $(".stop").each(function() {
                if ($(this).data("stopid") === stop.id) {
                    $(this).find(".btn-flip").remove();
                }
            });
        }

        getDistanceFrom(stop.address, next, function(resp) {
            nextContainer.find(".loading").hide();
            nextContainer.find(".loaded").show();
            nextContainer.find(".mileage").text(resp.distance);
            nextContainer.find(".hours").text(Utility.formatHours(resp.time));
        });
    }

    function getDistanceFrom(a, b, callback) {
        var url = SERVER.baseUrl + "Address/GetDistance";
        $.get(url, { address1id: a.AddressID, address2id: b.AddressID }, callback);
    }
})(window);