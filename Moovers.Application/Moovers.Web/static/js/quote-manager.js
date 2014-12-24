/*global Quote, Utility, PagedResult,Keys,_,SERVER */
/// <reference path="references.js" />
/*!
quote-manager.js
*/
(function(window, undefined) {
    "use strict";
    $.ajaxSetup({ cache: false });
    window.QuoteManager = {
        dialogs: {
            status: "#status-modal",
            owner: "#change-owner-modal"
        },
        page: 0,
        totalPages: 0,
        templates: {
            quote: "#quote-lookup",
            filtersApplied: "#filter-applied",
            stats: "#stat-boxes"
        },
        elements: {
            quoteTable: "#quote-table",
            ownerSelect: "#quote-table-actions select[name=quote-owner]",
            statusSelect: "#quote-table-actions select[name=quote-status]",
            typeSelect: "#quote-table-actions select[name=quote-type]",
            timeSelect: "#quote-table-actions select[name=quote-time]",
            filters: "#quote-table-actions select",
            filtersApplied: "ul.filters-applied",
            statTable: "#table-stats",
            search: "#search-cases"
        },

        // formatting for the "STATS"
        stats: {
            open: {
                Name: "open",
                BaseQuery: "open",
                Description: "open",
                SubStatuses: {
                    unassigned: {
                        Name: "unassigned",
                        BaseQuery: "open AND unassigned",
                        Description: "unassigned"
                    },
                    expired: {
                        Name: "expired",
                        BaseQuery: "expired",
                        Description: "expired"
                    }
                }
            },
            scheduled: {
                Name: "scheduled",
                BaseQuery: "scheduled",
                Description: "scheduled",
                SubStatuses: {}
            },
            lost: {
                Name: "lost",
                BaseQuery: "lost AND in the last 30 days",
                Description: "lost last 30 days",
                SubStatuses: {}
            },
            won: {
                Name: "won",
                BaseQuery: "won AND in the last 30 days",
                Description: "won last 30 days",
                SubStatuses: {}
            }
        },

        // # of quotes to show per page
        getTake: function() { return parseInt(localStorage["quote-mgr-take"] || 25, 10); },
        setTake: function(val) { localStorage["quote-mgr-take"] = val; },
        
        // QuoteSort -- field to sort by
        getSort: function() { return localStorage["quote-mgr-sort"] || ""; },
        setSort: function(val) { localStorage["quote-mgr-sort"] = val; },

        // Ascending/Descending sort
        getDesc: function() {
            if (_.isUndefined(localStorage["quote-mgr-desc"])) {
                return true;
            }
            return Utility.parseBool(localStorage["quote-mgr-desc"] || "");
        },
        setDesc: function(val) { localStorage["quote-mgr-desc"] = val; },

        // search query to filter by
        getCachedquery: function() { return localStorage["quote-mgr-query"] || ""; },
        setCachedquery: function(val) { localStorage["quote-mgr-query"] = val; },

        // Form fields -- these filter search results
        getSearch: function() {
            return this.elements.search.val();
        },

        
        getOwner: function() {
            return this.elements.ownerSelect.val();
        },

        /** see "Utility/GetAspUserFromSearch" -- AspUser search values have a "*" prepended to them */
        getDisplayOwner: function() {
            return this.elements.ownerSelect.val().replace("*", "");
        },

        getStatus: function() {
            return this.elements.statusSelect.val();
        },

        getType: function() {
            return this.elements.typeSelect.val();
        },

        getTime: function() {
            return this.elements.timeSelect.val();
        },

        // When we edit a quote using the actions menu, we need to save the search results and page we're on
        getSavedPage: function() {
            return parseInt(localStorage["quote-mgr-savepage"], 10);
        },
        setSavedPage: function(page) {
            if (!page) {
                localStorage.removeItem("quote-mgr-savepage");
            }
            else {
                localStorage["quote-mgr-savepage"] = page;
            }
        },
        getSavedSearch: function() {
            return localStorage["quote-mgr-savesearch"];
        },
        setSavedSearch: function(search) {
            if (!search) {
                localStorage.removeItem("quote-mgr-savesearch");
            }
            else {
                localStorage["quote-mgr-savesearch"] = search;
            }
        },

        showLoading: function() {
            $("#quote-table-container-loading").show();
        },

        hideloading: function() {
            $("#quote-table-container-loading").hide();
        },

        // the "crumbs" we use with search results, displayed in #filters-applied
        updateFilterDisplay: function() {
            var that = this;
            var vals = [
                { category: "quote-owner", name: this.getDisplayOwner() },
                { category: "quote-status", name: this.getStatus() },
                { category: "quote-type", name: this.getType() },
                { category: "quote-time", name: this.getTime() },
                { category: "quote-search", name: this.getSearch() ? "keyword: " + this.getSearch() : "" }
            ];

            this.elements.filtersApplied.empty();
            vals.forEach(function(i) {
                if (i.name) {
                    var tmp = that.templates.filtersApplied(i);
                    that.elements.filtersApplied.append(tmp);
                }
            });
        },

        // updates the selected "Stats" field when we change statuses
        updateStatusSelection: function() {
            var status = this.getStatus();
            var selectors = this.elements.statTable.find(".stat-selector").removeClass("selected");

            // "won" and "lost" are unique, in that they have additional requirements before they show as "selected"
            if (status === "won" || status === "lost") {
                if (this.getTime() === "in the last 30 days") {
                    selectors.filter("." + status).addClass("selected");
                }
            }
            else if (status) {
                selectors.filter("." + status).addClass("selected");
            }
        },

        // accepts a search query, in the form "field AND field, and applies it to the search results
        setFilters: function(query) {
            this.elements.filters.reset();
            query = query || "";

            var queryEls = query.split("AND").map(function(i) {
                return $.trim(i);
            });

            this.elements.filters.each(function() {
                var el = $(this);
                var opts = el.find("option");
                opts.each(function() {
                    var val = $(this).attr("value");
                    if (_.contains(queryEls, val)) {
                        el.val(val);
                    }
                });
            });

            this.elements.filters.selectpicker("render");
            this.elements.filters.filter(":first").trigger("change");

            if (!query) {
                return;
            }
        },
        // turns our form fields into a query to push to the server
        getQuery: function() {
            // unassigned is a special case -- it is a "status" filter, that actually only applies to "owners"
            var owner = (this.getStatus() === "unassigned") ? "unassigned" : this.getOwner();
            var options = [
                owner,
                this.getStatus(),
                this.getType(),
                this.getTime(),
                this.getSearch()
            ];

            options = _.filter(options, function(i) { return !!i; });
            return options.join(" AND ");
        },

        // our statistics are based off only move type and user -- form a query based on these fields to get statistics
        getStatQuery: function() {
            var options = [
                this.getOwner(),
                this.getType(),
                this.getSearch()
            ];

            options = _.filter(options, function(i) { return !!i; });
            return options.join(" AND ");
        },

        init: function(querySearch) {
            Utility.initBase(this);
            var mgr = this;

            // last run statistics query -- fairly often this doesn't change, even when filters are changed.
            var statQuery;

            // element bindings
            this.elements.filters.on("change", function() {
                mgr.page = 0;
                mgr.total = 0;
                updateQuotes();
            });

            if (querySearch) {
                mgr.setFilters("");
                mgr.elements.search.val(querySearch);
            }

            this.elements.filtersApplied.on("click", "li", function() {
                var category = $(this).data("category");

                // for "search", clear the "search box", otherwise clear the filter dropdown
                var element = (category === "quote-search") ? mgr.elements.search : mgr.elements.filters.filter("[name=" + category + "]");
                element.reset();
                element.selectpicker("render");
                element.trigger("change").trigger("search");
                return false;
            });

            // update "take" -- modifies how many quotes to show on a page
            $(".show-qty").on("click", function(e) {
                var target = $(e.target).closest("[data-qty]");
                if (target.length === 0 || target.is(".inactive")) {
                    return false;
                }

                var qty = parseInt(target.data("qty"), 10);
                mgr.setTake(qty);
                mgr.page = 0;
                updateQuotes();
                updatePageCountLinks();
                return false;
            });

            // forward/backward paging
            $(".page-stepper").on("click", function(e) {
                var target = $(e.target).closest(".page-selector");
                if (target.is(".inactive")) {
                    return false;
                }
                if (target.is(".next-page")) {
                    mgr.page++;
                }
                else if (target.is(".previous-page")) {
                    mgr.page--;
                }
                else if (target.is(".first-page")) {
                    mgr.page = 0;
                }
                else if (target.is(".last-page")) {
                    mgr.page = mgr.totalPages - 1;
                }

                updateQuotes();
            });

            $("#print").click(function() {
                window.print();
            });

            // setup various form elements
            $(".search-mag").click(function() {
                mgr.elements.search.trigger("search");
            });

            this.elements.search.on("keypress", function(e) {
                if (e.target === Keys.ENTER) {
                    $(this).trigger("search");
                }
            });

            this.elements.search.on("search", function() {
                mgr.page = mgr.total = 0;
                updateQuotes();
            });

            // when we change the "owner", save the current page and current search before we submit -- on reload, we'll keep the user on the same page.
            this.dialogs.owner.add(this.dialogs.status).find("form").on("submit", function() {
                mgr.setSavedPage(mgr.page);
                mgr.setSavedSearch(mgr.getSearch());
            });

            // essentially a javascript version of our "sortable table header"
            this.elements.quoteTable.find("thead tr.sort-col").on("click", "th a", function() {
                var el = $(this);
                var desc = Utility.parseBool(el.data("desc"));
                mgr.setSort(el.data("sort"));
                mgr.setDesc(desc);

                mgr.elements.quoteTable.find("thead th")
                    .removeClass("sel")
                    .removeClass("desc")
                    .find("a")
                    .data("desc", false);

                var th = el.closest("th");
                th.addClass("sel");
                th.toggleClass("desc", desc);
                el.data("desc", !desc);

                updateQuotes();
                return false;
            });

            // when a quote is clicked, go to that quote
            this.elements.quoteTable.on("click", "tr[data-quoteid]", function(e) {
                var target = $(e.target);
                if (target.closest(".btn-group").length === 0) {
                    var quoteid = $(this).data("quotelookup");
                    var url = SERVER.baseUrl + "Quote/Overview/" + quoteid;
                    Utility.showOverlay();
                    window.location = url;
                }
            });

            // hook up quote actions that can be completed on the action dropdown
            this.elements.quoteTable.on("click", "tr[data-quoteid] .btn-group", function(e) {
                var target = $(e.target).closest("[data-action]");
                var quoteid = $(this).closest("[data-quoteid]").data("quoteid");
                var lookup = $(this).closest("[data-quoteid]").data("quotelookup");
                if (target.length === 0) {
                    return;
                }

                var action = target.data("action");

                // all of these actions work on the "change status" modal,
                // POST /Quote/ChangeStatus
                var statuses = ["defer", "close", "duplicate", "reopen"];
                statuses.forEach(function(i) {
                    if (action === i) {
                        var dialog = mgr.dialogs.status;
                        var desc = target.data("desc");
                        dialog.find(".title").text(desc + " Quote " + lookup);
                        dialog.find("[name=action]").val(i.capitalize());
                        dialog.find("[type=submit]").val(desc + " Quote");
                        showModal(mgr.dialogs.status, quoteid);
                        mgr.setSavedPage(mgr.page);
                    }
                });

                // redirect to "account lookup"
                if (action === "view-account") {
                    var accountlookup = target.closest("[data-accountlookup]").data("accountlookup");
                    var url = SERVER.baseUrl + "Accounts/Index/" + accountlookup;
                    Utility.showOverlay();
                    window.location = url;
                }

                // show a "change owner" modal
                if (action === "change-owner") {
                    showModal(mgr.dialogs.owner, lookup);
                }

                // special case of "change owner" -- fill out the form and submit it for the current user
                if (action === "change-owner-self") {
                    Utility.showOverlay();
                    var modal = mgr.dialogs.owner;
                    var userbox = $("<input type='text' name='userid'>").val(SERVER.userid);
                    modal.find("[name=userid]").replaceWith(userbox);
                    modal.find("[name=id]").val(lookup);
                    modal.find("[name=redirect]").val(SERVER.baseUrl + "Quote/Overview/" + lookup);
                    modal.find("form").submit();
                }

                return false;
            });

            // hook up ability to click between "stats"
            this.elements.statTable.on("click", ".stat-selector", function(e) {
                var item = $(e.target).closest(".stat-selector");
                var query = item.data("query");
                mgr.setFilters(query);
                mgr.elements.statTable.find(".stat-selector").removeClass("selected");
                item.addClass("selected");
                updateQuotes();
            });

            // reset all filtering
            $(".filter-reset").click(function() {
                mgr.elements.search.val("");
                mgr.setFilters("");
            });

            // initialize default states of all elements
            $(".selectpicker").selectpicker();

            // sort is persistent across page loads, properly set the initial class of table header links
            var el = this.elements.quoteTable.find("a[data-sort=" + mgr.getSort() + "]");
            if (el.length > 0) {
                this.elements.quoteTable.find("th").removeClass("sel desc");
                el.closest("th").addClass("sel").toggleClass("desc", mgr.getDesc());
                el.data("desc", !mgr.getDesc());
            }

            // page count is also persistent across pageload
            if (Utility.isJson(mgr.getCachedquery())) {
                var obj = JSON.parse(mgr.getCachedquery());
                mgr.setCachedquery(undefined);
                mgr.setFilters(obj.search);
            }

            // we don't always want to save the current "page", but occasionally we do, catch that situation
            if (mgr.getSavedPage()) {
                mgr.page = mgr.getSavedPage();
                mgr.setSavedPage(undefined);
            }
            if (mgr.getSavedSearch()) {
                mgr.elements.search.val(mgr.getSavedSearch());
                mgr.setSavedSearch(undefined);
            }

            updateQuotes();
            updateStats();
            updatePageCountLinks();

            function showModal(modal, quoteid) {
                modal.find("[name=id]").val(quoteid);
                modal.modal("show");
            }

            // utility functions to handle pagination
            function updatePageCountLinks() {
                $(".pagination").find("a").removeClass("active").filter("[data-qty=" + mgr.getTake() + "]").addClass("active");
            }

            function updatePageLinks() {
                mgr.page = Math.max(mgr.page, 0);
                mgr.page = Math.min(mgr.totalPages - 1, mgr.page);
                $(".next-page,.last-page").toggleClass("inactive", mgr.page >= mgr.totalPages - 1);
                $(".previous-page,.first-page").toggleClass("inactive", mgr.page <= 0);
            }

            function updateStats(cb) {
                cb = cb || $.noop;

                // most of the time, filtering doesn't change the stats we show
                var newQuery = mgr.getStatQuery();
                if (statQuery === newQuery) {
                    cb();
                    return;
                }

                statQuery = newQuery;
                $.getq("stat-manager", SERVER.baseUrl + "Quote/GetStats", { search: statQuery }, function(resp) {
                    mgr.stats.open.Count = resp.OpenCount;
                    mgr.stats.open.Amount = resp.OpenAmount;
                    mgr.stats.scheduled.Count = resp.ScheduledCount;
                    mgr.stats.scheduled.Amount = resp.ScheduledAmount;
                    mgr.stats.lost.Amount = resp.Lost30DaysAmount;
                    mgr.stats.lost.Count = resp.Lost30DaysCount;
                    mgr.stats.won.Count = resp.Won30DaysCount;
                    mgr.stats.won.Amount = resp.Won30DaysAmount;
                    mgr.stats.open.SubStatuses.expired.Count = resp.ExpiringCount;
                    mgr.stats.open.SubStatuses.unassigned.Count = resp.UnassignedCount;

                    _.each(mgr.stats, function(i) {
                        var setQuery = function(item) {
                            item.Query = item.BaseQuery;
                            if (statQuery !== "") {
                                item.Query += " AND " + statQuery;
                            }
                        };

                        setQuery(i);
                        _.each(i.SubStatuses, setQuery);
                    });

                    mgr.elements.statTable.empty();
                    $.each(mgr.stats, function(i, j) {
                        var tmpl = mgr.templates.stats(j);
                        mgr.elements.statTable.append(tmpl);
                    });

                    cb();
                });
            }

            // retemplate quotes and append to the "quote-table"
            function updateQuotes() {
                var url = SERVER.baseUrl + "Quote/Index";
                var query = { search: mgr.getQuery(), page: mgr.page, take: mgr.getTake(), sort: mgr.getSort(), desc: mgr.getDesc() };
                
                if (JSON.stringify(query) === mgr.getCachedquery()) {
                    // if our search hasn't changed, just update stats if required
                    finished();
                    return;
                }

                mgr.setCachedquery(JSON.stringify(query));
                mgr.showLoading();
                mgr.updateFilterDisplay();

                var storage = updateStorage(query);
                var quotes = $.getq("quote-manager", url, query, function(resp) {
                    var quotes = resp.Quotes.map(function(i) { return new Quote(i); });
                    var pagination = new PagedResult(resp.PagedResult);

                    var tbody = mgr.elements.quoteTable.find("tbody").empty();

                    quotes.forEach(function(i) {
                        tbody.append(mgr.templates.quote(i));
                    });

                    $(".current-page").text(pagination.Description);
                    mgr.totalPages = pagination.PageCount;
                    updatePageLinks(mgr.totalPages);
                });

                $.when(storage, quotes).then(function() {
                    if (!$.ajaxq.isRunning("quote-manager")) {
                        mgr.hideloading();
                    }
                });

                finished();

                function finished() {
                    updateStats(function() {
                        mgr.updateStatusSelection();
                    });
                }
            }


            var storageTemplate = _.template($("#storage-account").html());
            function updateStorage(query) {
                var url = SERVER.baseUrl + "Quote/GetStorageAccounts";
                return $.postq("quote-manager", url, query, function(resp) {
                    var accounts = resp;
                    var markup = resp.map(function(r) {
                        return storageTemplate(r);
                    });
                    console.log(markup);

                    $("#storage-table tbody").html(markup);
                    $("#storage-table-container").toggle(accounts.length > 0);
                });
            }
        }
    };
})(window);