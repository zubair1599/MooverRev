// <reference path="references.js" />
/*global Utility,Repository,PostingTemplates,PaymentModal,Posting,Keys,PostingPage,SearchFunctions*/
/*!
  post.js
*/
(function(exports) {
    exports.PostingTemplates = {
        templates: {
            posting: "#posting-template",
            vehicle_rel: "#vehicle-rel-template",
            employee_rel: {
                element: "#employee-rel-template",
                options: { variable: "rel" }
            },
            moving_service_rel: "#moving-service-template",
            posting_service: "#posting-service-template"
        },
        opts: {
            getPost: function() { },
            onSubmit: function() { },
            onTemplate: function() { }
        },
        onTemplate: function(container) {
            var addEmployee = container.find("[name=add-employee]");
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
                onSelect: function(/*val, e*/) {
                    $(this).removeClass("error");
                    addEmployee.autocomplete("hide");
                    container.find(".add-employee-post").click();
                    container.find("[name=add-employee]").focus();
                    return false;
                }
            });

            var addVehicle = container.find("[name=add-vehicle]");
            addVehicle.autocomplete({
                lookup: _.map(Repository.vehicles, function(c) {
                    return {
                        value: c.getDisplayValue(),
                        alts: [c.Lookup, c.Name]
                    };
                }),
                partialMatch: true,
                selectFirst: true,
                onSelect: function(/*val, e*/) {
                    addVehicle.autocomplete("hide");
                    container.find(".add-vehicle-post").click();
                    container.find("[name=add-vehicle]").focus();
                    return false;
                },
                maxSuggestions: 10,
                orderBy: SearchFunctions.vehicleAutocompleteSort
            });

            addEmployee.add(addVehicle).bind("keydown", function(e) {
                if (e.keyCode === Keys.ENTER) {
                    $(this).addClass("error");
                    $(this).select();
                    return false;
                }
            });

            if ($.isFunction(PostingTemplates.opts.onTemplate)) {
                PostingTemplates.opts.onTemplate(container);
            }
        },
        load: function(container, opts) {
            Utility.initBase(PostingTemplates);
            PostingTemplates.opts = $.extend({}, PostingTemplates.opts, opts);

            function syncPosts() {
                if (!$.ajaxq.isRunning()) {
                    var posts = _(PostingTemplates.opts.getPost()).filter(function(p) { return p.hasChanged; });
                    _.each(posts, function(p) { p.sync(); });
                }

                setTimeout(function() {
                    syncPosts();
                }, 250);
            }

            syncPosts();

            container.on("blur", "input[type=number]", function() {
                var val = parseFloat($(this).val());
                if (val) {
                    $(this).val(Utility.formatCurrency(val, "", ""));
                }
            });

            container.on("click", ".icon-remove", function() {
                var link = $(this);
                var posting = PostingTemplates.opts.getPost()[0];
                posting.hasChanged = true;
                if (link.is(".employee")) {
                    var employeeid = link.closest("tr").data("employeeid");
                    posting.removeEmployee(Repository.getEmployee(employeeid), false);
                    posting.retemplate();
                }
                else if (link.is(".vehicle")) {
                    var vehicleid = link.closest("tr").data("vehicleid");
                    posting.removeVehicle(Repository.getVehicle(vehicleid), false);
                    posting.retemplate();
                }

                return false;
            });

            container.on("click", ".sibling-show-employees", function() {
                var icon = $(this).find(".icon-double-angle-down,.icon-double-angle-up");
                var postingid = icon.data("postingid");

                icon.toggleClass("icon-double-angle-up icon-double-angle-down");
                icon.closest("table").find("tr").each(function() {
                    var tr = $(this);
                    if (tr.data("postingid") === postingid) {
                        tr.toggleClass("hide");
                    }
                });
            });

            container.on("blur", "input[name=Commission_Percent]", function() {
                var row = $(this).closest("tr");
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var val = parseFloat($(this).val());
                var price = post.calculateMovingCost();

                var commission = Utility.roundCurrency(Utility.roundTo(val / 100, 4) * price);
                row.find("[name=Commission]").val(commission).trigger("change");
            });

            container.on("blur", "input[name=Commission]", function() {
                var row = $(this).closest("tr");
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var val = parseFloat($(this).val());
                var price = post.calculateTotalCost();
                var percent = Utility.roundCurrency((val / price) * 100);
                row.find("[name=Commission_Percent]").val(percent);
            });

            container.on("blur", "input[name=duration],input[name=driveHours]", function() {
                var container = $(this).closest(".posting-container");
                var post = PostingTemplates.opts.getPost(container.data("postingid"));

                var row = $(this).closest("tr");
                var hours = parseFloat(row.find("input[name=duration]").val());
                var drive = parseFloat(row.find("input[name=driveHours]").val());
                var employeeids = _.pluck(post.Employee_Rels, "EmployeeID");
                _.each(employeeids, function(id) {
                    post.setEmployeeField(id, "Hours", (hours + drive));
                });
                post.retemplateEmployees();
            });

            container.on("blur change", "input", function() {
                var field = $(this).data("field");
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var isCustom = Utility.parseBool($(this).data("iscustom")) === true;
                var isEmployee = !!$(this).data("employeeid");

                if (field && post) {
                    var val = $(this).val();

                    val = !isNaN(Number(val)) ? Number(val) : val;

                    if ($(this).is(":checkbox")) {
                        val = $(this).is(":checked");
                    }

                    if (isCustom) {
                        post.setCustomField(field, val);
                    }
                    else if (isEmployee) {
                        var employeeid = $(this).data("employeeid");
                        post.setEmployeeField(employeeid, field, val);
                    }
                    else {
                        post.setField(field, val);
                    }
                }

                return true;
            });

            container.on("click", ".custom-service .icon-remove", function() {
                var serviceid = $(this).closest(".custom-service").find("input[name=serviceid]").val();
                var postid = $(this).closest(".posting-container").data("postingid");
                var post = PostingTemplates.opts.getPost(postid);
                post.removeService(serviceid);
                return false;
            });

            container.on("click", "input", function() {
                $(this).select();
                $(this).focus();
            });

            container.on("click", "button.add-custom-service", function() {
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var val = $(this).closest("tr").find("input.custom-description").val();
                post.addCustomService(val);
            });

            container.on("click", ".add-employee-post", function() {
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var emp = Repository.getEmployee(container.find("[name=add-employee]").val());
                post.addEmployee(emp);
                post.retemplate();
            });

            container.on("click", ".add-vehicle-post", function() {
                var post = PostingTemplates.opts.getPost($(this).closest(".posting-container").data("postingid"));
                var vehicle = Repository.getVehicle(container.find("[name=add-vehicle]").val());
                post.addVehicle(vehicle);
                post.retemplate();
            });

            container.on("click", "button.btn-post", function() {
                var postingid = $(this).closest(".posting-container").data("postingid");
                var post = PostingTemplates.opts.getPost(postingid);

                if (window.confirm("Are you sure you'd like to post this job?")) {
                    // save any pending changes
                    var loader = post.container.find(".ajax-loader").show();
                    post.sync(function() {
                        // mark the post as completed
                        var url = SERVER.baseUrl + "PostingPage/CompletePost/";
                        var data = { id: post.PostingID };
                        var req = $.postq("posting", url, data, function() {
                            window.location = SERVER.baseUrl + "Posting";
                        });
                        req.fail(function() {
                            loader.hide();
                        });
                    });
                }

                return false;
            });
        }
    };

    exports.PostingPage = {
        curPost: [],
        getPosting: function(id) {
            if (!id) {
                return [PostingPage.curPost];
            }
            
            return PostingPage.curPost;
        },
        init: function(postid) {
            PaymentModal.init($("#add-payment-modal"));
         
            $("#container").on("click", "#add-payment", function() {
                PaymentModal.show();
                return false;
            });

            PostingTemplates.load($("#container"), { getPost: PostingPage.getPosting });
            Repository.init(window.EMPLOYEES, window.VEHICLES);

            PostingPage.changePost(postid);
        },
        changePost: function(postid) {
            var loader = $("#post-selector-loader").show();
            var url = SERVER.baseUrl + "PostingPage/GetPostingJson/";

            var post = $.postq("postingtemplates", url + postid, function(resp) {
                var json = resp;
                var post = window.asdf = PostingPage.curPost = new Posting(json, $("#container"));
                $("#container").data("postingid", post.PostingID);
                post.retemplate();
            });

            post.always(function() {
                loader.hide();
            });
        }
    };
})(window);