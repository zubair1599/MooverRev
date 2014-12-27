/// <reference path="references.js" />
/*!
    schedule.js
*/
/*global QuoteScheduleDay,Keys,Utility,Repository,Assignment,PaymentModal,SearchFunctions,Kalendae*/

(function(exports) {
    var baseurl;

    function goToDay(date) {
        var day = date.getDate();
        var month = date.getMonth();
        var year = date.getFullYear();
        var url = baseurl + "?day=" + day + "&month=" + (month + 1) + "&year=" + year;
        window.location = url;
    }

    exports.Schedule = {
        init: function(url, height, highlightDays, franchiseid) {
            baseurl = url;
            height = height || 700;
            highlightDays = highlightDays || [];

            $("#schedule").fullCalendar({
                dayClick: function(date) {
                    goToDay(date);
                    return false;
                },
                eventClick: function(evt) {
                    goToDay(evt.start);
                    return false;
                },

                highlightDays: highlightDays,

                height: height,
                events: {
                    url: SERVER.baseUrl + "Quote/GetSchedule?franchiseid=" + franchiseid,
                    cache: true
                },
                buttonClick: function() {
                    var date = $("#schedule").fullCalendar("getDate");
                    var nextMonth = new Date(date.setMonth(date.getMonth() + 1));
                    $("#schedule2").fullCalendar("gotoDate", nextMonth);
                }
            });

            $("#schedule2").fullCalendar({
                dayClick: function(date) {
                    goToDay(date);
                    return false;
                },

                highlightDays: highlightDays,
                eventClick: function(evt) {
                    goToDay(evt.start);
                    return false;
                },
                height: height,
                events: {
                    url: SERVER.baseUrl + "Quote/GetSchedule" + (franchiseid ? "?franchiseid=" + franchiseid : ""),
                    cache: true
                }
            }).fullCalendar("next");


        }
    };

    exports.QuoteScheduleDay = {
        urls: {
            saveNote: SERVER.baseUrl + "Quote/AddNote"
        },
        init: function(url, ismobile) {
            baseurl = url;
            Assignment.init();

            var scheduleModal = $("#schedule-modal").modal({ show: false });
            $(".schedule-table").on("click", ".schedule-job", function () {
                if ($(this).is("[data-addTruck]")) {
                    var quoteid = $(this).data("quoteid");
                    var crew = $(this).data("crew");
                    var day = $(this).data("day");
                    var url = SERVER.baseUrl + "Quote/ScheduleJobNoPayment";
                    Utility.showOverlay();
                    var ajax = $.post(url, { quoteid: quoteid, crew: crew, day: day });
                    ajax.always(function() {
                        window.location.reload();
                    });
                    return;
                }

                var basetime = $(this).data("minutes");
                var hours = Math.floor(basetime / 60);
                var minutes = Math.ceil(basetime % 60);
                var data = {
                    hours: hours,
                    minutes: minutes,
                    crew: $(this).data("crew")
                };

                Utility.objectToForm(data, scheduleModal);
                scheduleModal.modal("show");
                scheduleModal.find("input:visible,select,textarea").first().focus();
            });

            $("#new-card").hide();
            $("input[name=paymentType]").change(function() {
                var newCard = $(this).val() === "NEW_CARD";
                $("#new-card").toggle(newCard);
                $("#new-card").find("[name=name]").toggleAttr("required", newCard);
                $("#new-card").find("[name=cardnumber]").toggleAttr("required", newCard);
            });

            scheduleModal.find("form").submit(function() {
                function showError(err) {
                    scheduleModal.find(".errors").empty().show().append(err);
                }

                var submit = $(this).find("[type=submit]").attr("disabled", "disabled");
                var loader = $(this).find("#cc-submit-loader").show();
                var url = $(this).attr("action");
                var data = $(this).serialize();
                var ajax = $.post(url, data);

                ajax.error(function() {
                    showError("Error processing request");
                });
                ajax.success(function(resp) {
                    if (resp.Errors) {
                        showError(resp.Errors[0].ErrorMessage);
                        return;
                    }
                    window.location.reload();
                });

                ajax.always(function() {
                    loader.hide();
                    submit.removeAttr("disabled");
                });

                return false;
            });

            var editModal = $("#edit-schedule").modal({ show: false });
            $(".icon-edit.edit-schedule").click(function() {
                var totalMinutes = $(this).data("minutes");
                var hours = Math.floor(totalMinutes / 60);
                var minutes = totalMinutes % 60;
                var data = {
                    scheduleid: $(this).data("scheduleid"),
                    minutes: minutes,
                    hours: hours,
                    rangestart: $(this).data("start"),
                    rangeend: $(this).data("end"),
                    movers: $(this).data("movers"),
                    crew: $(this).data("crew")
                };

                Utility.objectToForm(data, editModal);
                editModal.modal("show");
            });

            var modal = $("#change-card-on-file-modal").modal({ show: false });
            modal.find("form").on("submit", function(e) {
                var data = $(this).serialize();
                var url = $(this).attr("action");
                Utility.showOverlay();

                var showError = function(error) {
                    $("#error-change-card").text(error.ErrorMessage).show();
                };

                $.post(url, data, function(e) {
                    if (e.Errors && e.Errors.length > 0) {
                        showError(e.Errors[0]);
                        Utility.hideOverlay();
                    }
                    else {
                        window.location.reload();
                    }
                });

                e.preventDefault();
            });

            $("#change-card-on-file").click(function() {
                modal.modal("show");
            });
          
            $("#print-form").submit(function() {
                $(".modal.in").modal("hide");
            });

            var goDate = function() {
                var date = Kalendae.moment($(this).val()).toDate();
                goToDay(date);
            };

            $("#schedule-day-picker").bind("change select", goDate);

            var modals = $(".add-payment-modal").map(function() {
                var modal = $(this);
                PaymentModal.init(modal);
                return {
                    id: modal.data("scheduleid"),
                    modal: modal
                };
            }).get();

            $(".schedule-table").on("click", ".icon-money", function() {
                var id = $(this).data("scheduleid");
                _.find(modals, function(i) {
                    return i.id === id;
                }).modal.modal("show");
            });


            // disable functionality that is difficult to use on mobile devices.
            if (ismobile) {
                return;
            }

            (function() {

                // we only want to allow sorting between lists, so we "cancel" the sort operation unless an item was moved to a different table
                // this is a global variable that stores whether an item has been moved to a different list
                var hasBeenReordered = false;

                var table = $(".schedule-table-container").sortable({
                    connectWith: ".schedule-table-container",
                    handle: ".reorder span",
                    items: "tbody > .quote",
                    axis: "y",
                    start: function() {
                        $(".schedule-table tbody").addClass("drop");
                    },
                    stop: function(/*e, ui*/) {
                        if (!hasBeenReordered) {
                            $(".schedule-table-container").sortable("cancel");
                        }

                        hasBeenReordered = false;
                        $(".schedule-table-container tbody").removeClass("drop");
                    },
                    helper: function(e, ui) {
                        var clone = $(ui.clone());
                        clone.children("td").css("display", "table-cell");

                        var i = 0;
                        ui.children().each(function() {
                            var item = $(this);
                            $(clone.children().index(i)).width(item.width());
                            i++;
                        });

                        return clone;
                    },
                    // receive callback is called when an item is moved between lists
                    receive: function(e, ui) {
                        hasBeenReordered = true;
                        $("#save-day").slideDown();
                        $(".schedule-job").hide();
                        $(ui.item).appendTo($(e.target).find("tbody"));
                        var crewid = $(ui.item).closest("table").data("crewid");
                        $(ui.item).find("input[name=crewid]").val(crewid);
                    }
                });

                if (!$.browser.mozilla) {
                    table.disableSelection();
                }
                    
            })();


            var statuses = $("[name=crewstatus]:first option").map(function() {
                return $(this).val().toLowerCase();
            });

            $("[name=crewstatus]").change(function() {
                var item = $(this);
                var container = item.closest(".schedule-table-container");
                var val = item.val().toLowerCase();
                _.each(statuses, function(i) {
                    container.removeClass(i);
                });
                container.addClass(val);
                $("#save-day").slideDown();
            });

            $("#save-day").click(function() {
                $("#save-order-submit").click();
            });

            var defaultText = "Add Note";
            var dataField = "initial-text";
            $(".note").on("click", "a.add-note", function() {
                var a = $(this);
                if (a.length > 0) {
                    var parent = a.parent();
                    var input = parent.find("input.note-text");
                    input.data(dataField, a.text());
                    parent.addClass("edit");
                    input.val(a.text()).focus().select();
                    return false;
                }
            });

            $(".note").on("blur", "input.note-text", function() {
                var txt = $(this);
                console.log("HERE");
                var parent = txt.parent();
                var link = parent.find("a.add-note");
                var val = $.trim(txt.val());

                if (val === defaultText) {
                    val = "";
                }

                var id = txt.closest(".quote").data("scheduleid");
                $.postq("AddNote", QuoteScheduleDay.urls.saveNote, { scheduleid: id, note: val }, $.noop);

                txt.removeData(dataField);
                parent.removeClass("edit");

                var allLinks = $("tr[data-scheduleid=" + id + "]").find("a.add-note");
                allLinks.text(val || defaultText).toggleClass("changed", link.text() !== defaultText);
            });

            $(".note").on("keyup", "input.note-text", function(e) {
                var txt = $(this);
                if (e.keyCode === Keys.ENTER) {
                    txt.closest("tr").find(".cursor-holder").focus();
                    return false;
                }
                if (e.keyCode === Keys.ESCAPE) {
                    var initial = txt.data(dataField);
                    txt.val(initial);
                    txt.removeData(dataField);
                    txt.blur();
                    return false;
                }
            });
        }
    };

    exports.Assignment = {
        templates: {
            crewAssign: "#crew-assign-template",
            vehicleAssign: "#vehicle-assign-template"
        },
        dialogs: {
            main: "#assign-resources-modal"
        },
        init: function() {
            Utility.initBase(exports.Assignment);
            $(".btn-assign").click(function() {
                Assignment.dialogs.main.modal("show");
                var crew = $(this).data("crew");
                var curCrew = $(this).closest("table").find(".emp-holder [data-employeeid]").map(function() {
                    var tbl = $(this);
                    return {
                        id: tbl.data("employeeid"),
                        isdriver: Utility.parseBool(tbl.data("isdriver"))
                    };
                }).get();

                var curVehicles = $(this).closest("table").find(".vehicle-holder [data-vehicleid]").map(function() {
                    return $(this).data("vehicleid");
                }).get();

                var empmarkup = _.map(curCrew, function(c) {
                    var emp = Repository.getEmployee(c.id);
                    emp.isdriver = c.isdriver;
                    return Assignment.templates.crewAssign(emp);
                }).join("");

                var vehicleMarkup = _.map(curVehicles, function(c) {
                    var vehicle = Repository.getVehicle(c);
                    return Assignment.templates.vehicleAssign(vehicle);
                });

                $("#crew-assignment").empty().append(empmarkup);
                $("#vehicle-assignment").empty().append(vehicleMarkup);
                Assignment.dialogs.main.find("[name=crewlookup]").val(crew);
                return false;
            });

            Assignment.dialogs.main.on("click", ".icon-remove", function() {
                $(this).closest(".crew-assign,.vehicle-assign").remove();
            });

            var emps = window.EMPLOYEE_JSON;
            var vehs = window.VEHICLE_JSON;
            Repository.init(emps, vehs);

            var addEmployee = $("#add-employee").focus();
            addEmployee.autocomplete({
                lookup: _.map(Repository.employees, function(c) {
                    return {
                        value: c.getDisplayValue(),
                        alts: [c.Lookup, c.NameFirst, c.NameLast]
                    };
                }),
                partialMatch: true,
                selectFirst: true,
                maxSuggestions: 10,
                orderBy: SearchFunctions.employeeAutocompleteSort,
                onSelect: function() {
                    $("#btn-add-employee").click();
                    return false;
                }
            });

            addEmployee.bind("keydown keypress keyup", function(e) {
                if (e.keyCode === Keys.ENTER) {
                    $("#btn-add-employee").click();
                    return false;
                }
            });

            var addVehicle = $("#add-vehicle");
            addVehicle.autocomplete({
                lookup: _.map(Repository.vehicles, function(c) {
                    return {
                        value: c.getDisplayValue(),
                        alts: [c.Lookup, c.Name]
                    };
                }),
                partialMatch: true,
                selectFirst: true,
                maxSuggestions: 10,
                orderBy: SearchFunctions.vehicleAutocompleteSort
            });

            addVehicle.bind("keydown keypress keyup", function(e) {
                if (e.keyCode === Keys.ENTER) {
                    $("#btn-add-vehicle").click();
                    return false;
                }
            });

            $("#btn-add-vehicle").click(function() {
                var vehicle = Repository.getVehicle(addVehicle.val());
                var form = $(this).closest("form");

                var existing = _.chain([form.serializeObject().vehicleid]).flatten().any(function(i) {
                    return i === (vehicle && vehicle.VehicleID);
                }).value();

                addVehicle.val("");
                addVehicle.autocomplete("hide");
                if (!vehicle || existing) {
                    return false;
                }

                $("#vehicle-assignment").append(Assignment.templates.vehicleAssign(vehicle));
                return false;
            });

            $("#btn-add-employee").click(function() {
                var emp = Repository.getEmployee(addEmployee.val());
                var form = $(this).closest("form");
                addEmployee.autocomplete("hide");

                addEmployee.val("");
                addEmployee.trigger("keyup");
                if (!emp) {
                    return false;
                }

                var onPage = $(".emp-holder [data-employeeid]").map(function() {
                    return $(this).data("employeeid");
                }).get();

                if (_.contains(onPage, emp.EmployeeID)) {
                    var currentlyAssigned = $("[data-employeeid=" + emp.EmployeeID + "]").closest("[data-crewid]").data("crewid");
                    if (!window.confirm("This employee is already assigned to Crew " + currentlyAssigned + " - Are you sure you want to add them to this crew?")) {
                        return false;
                    }
                }

                var existing = _.chain([form.serializeObject().employeeid]).flatten().any(function(i) {
                    return i === (emp && emp.EmployeeID);
                }).value();

                if (existing) {
                    return false;
                }

                emp.isdriver = false;
                $("#crew-assignment").append(Assignment.templates.crewAssign(emp));
                return false;
            });
        }
    };

    exports.ScheduleDay = {
        init: function() {
            var printmodal = $("#print-modal").modal({ show: false });

            $("#print-quotes").click(function () {
                printmodal.modal("show");
            });

            //$(".check-all-invoice").click(function() {
            //    var chkbxs = $("[type='checkbox'][name='invoice']");
            //    if ($(this).is('checked')) {
            //        chkbxs.attr('checked', true);
            //    } else {
            //        chkbxs.attr('checked', false);
            //    }
            //});

            //$("#print-form table").tableSorter();
        }
    };
})(window);