/// <reference path="references.js" />
/*!
    accounts.js
*/

/*global _,$,Utility,initVerifyAddress, Accounts,History,PersonAccount,BusinessAccount*/
window.Accounts = {
    currentPage: 0,
    fullCache: [],
    isLoadingMenu: false,
    urls: {
        search: SERVER.baseUrl + "Accounts/All/",
        getSingle: SERVER.baseUrl + "Accounts/Get/",
        addCard: SERVER.baseUrl + "Accounts/AddCard"
    },
    templates: {
        account: "#account-template",
        personHeader: "#personheader-account-template",
        businessHeader: "#businessheader-account-template",
        menu: "#account-menu-template"
    },
    prevMailing: {},
    elements: {
        accountContainer: "#account-container",
        menuContainer: "#account-menu",
        searchBox: "#account-menu-search",
        headerContainer: "#header-container",
        addCardButton: "#add-credit-card",
        addCardModal: "#add-card-modal"
    },
    refreshMenu: function(items, selected) {
        Accounts.isLoadingMenu = true;
        var templater = function(resp) {
            var menuContainer = Accounts.elements.menuContainer;

            var previousSelected = menuContainer.find(".selected").data("accountid");
            if (Accounts.currentPage === 0) {
                menuContainer.empty();
            }

            menuContainer.append(_.map(resp, function(acct) {
                return Accounts.templates.menu(acct);
            }));

            var auto = (selected === "auto");
            if (!selected) {
                selected = previousSelected;
            }
            if (auto) {
                selected = menuContainer.find(".account:first").data("accountid");
            }
          
            Accounts.showAccount(selected, auto);
            Accounts.isLoadingMenu = false;
        };

        if (!items) {
            var value = Accounts.elements.searchBox.val();
            $.getq("account", Accounts.urls.search, { q : value, page: Accounts.currentPage, _: $.now() }, templater);
        }
        else {
            templater(items);
        }
    },

    // show an account, by default wont load while another account is loading
    showAccount: function(id, isinitial, isfromstate) {
        if (!id ||  (!Accounts.isLoadingMenu && $.ajaxq.isRunning())) {
            return;
        }

        var menuRow = Accounts.elements.menuContainer.find(".account[data-accountid=" + id + "]");
        menuRow.addClass("selected").siblings().removeClass("selected");
        var item = _.first(_.filter(Accounts.fullCache, function(a) {
            return a.AccountID === id;
        }));

        var success = function(resp) {
            var acct = (resp.Type === "Person") ? new PersonAccount(resp) : new BusinessAccount(resp);
            var template = Accounts.templates.account;
            var headerTemplate = (resp.Type === "Person") ? Accounts.templates.personHeader : Accounts.templates.businessHeader;

            $("input[name=hiddenAccountID]").val(acct.AccountID);
            $("#same-as-billing-option").text(acct.DisplayName);

            Accounts.elements.accountContainer.empty().append(template(acct));
            Accounts.elements.headerContainer.empty().append(headerTemplate(acct));
            Accounts.elements.accountContainer.find("a.add-account").data("account-json", acct);
            Accounts.elements.headerContainer.find("a.add-account").data("account-json", acct);
            var url = (isinitial) ? window.location.href : SERVER.baseUrl + "Accounts/Index/" + acct.Lookup;
            if (!isfromstate) {
                History.pushState({ accountid: acct.AccountID }, "Account " + acct.Lookup, url);
            }
        };

        if (item) {
            success(item);
        } else {
            $.postq("account", Accounts.urls.getSingle + id, function(resp) {
                Accounts.fullCache.push(resp);
                success(resp);
            });
        }
    },

    businessDefaults: {},
    personDefaults: {},
    showModal: function(type, accountjson, opts) {
        accountjson = accountjson || {};
        var modal = (type === "Business") ? $("#create-business-dialog") : $("#create-person-dialog");
        var defaults = (type === "Business") ? Accounts.businessDefaults : Accounts.personDefaults;
        opts = $.extend({}, defaults, opts);

        var formBox = modal.find(".account-custom-form");
        var inputTemplate = _.template("<input name='{{- key }}' value='{{- val }}' />");
        if (opts.data) {
            var html =  _.map(opts.data, function(val, key) {
                return inputTemplate({ val: val, key: key });
            }).join("");

            formBox.empty().append($.parseHTML(html));
        }
        else {
            formBox.empty();
        }

        if (opts.action) {
            modal.find("form").attr("action", opts.action);
        }

        if (opts.method) {
            modal.find("form").attr("method", opts.method);
        }
        else {
            modal.find("form").attr("method", "post");
        }

        var infoBox = modal.find(".account-custom-info");
        if (opts.info) {
            infoBox.empty().show().append(opts.info);
        } else {
            infoBox.empty().hide();
        }

        modal.find(".ajax-loader").hide();
        modal.find(".errors").empty().hide();
        modal.find("input.error").removeClass("error");
        modal.find("input,select").each(function() {
            var field = $(this).data("field");
            if (!field) {
                return;
            }
            var vals = field.split(".");
            if (vals.length === 1) {
                $(this).val(accountjson[field]);
            } else if (accountjson[vals[0]]) {
                $(this).val(accountjson[vals[0]][vals[1]]);
            } else {
                $(this).val("");
            }
        });

        Accounts.prevMailing = accountjson.BillingAddress;
        if (accountjson.CopyMailing || $.isEmptyObject(accountjson)) {
            modal.find("input.copy-mailing").attr("checked", "checked");
            modal.find(".address-container .Billing").find("input,select").addClass("disabled").attr("disabled", "disabled");
        }

        modal.modal("show");
        modal.find(".page").hide();
        modal.find(".page1").show();
        return modal;
    },
    showError: function(form, msg) {
        form.find(".errors").empty().show().append("<strong>Error</strong>: " + msg);
    },
    saveForm: function(form, success, hideModalOnSave) {
        //var tmpData = form.serializeObject();
        var url = form.attr("action");
        var method = form.attr("method");
        var data = form.serializeObject();

        if (!data["address-select"]) {
            Accounts.showError(form, "Please select an address");
            return;
        }

        if (data["address-select"] === "CURRENT_SELECTED") {
            data.currentMailing = true;
        }
        else if (!JSON.parse(data["address-select"]).accountid) {
            data.verified = data["address-select"];
        }

        _.each(data, function(val, key) {
            if ($.isArray(val)) {
                data[key] = val.join("|");
            }
        });

        var submitButton = form.find(".save-form").addClass("disabled").attr("disabled", "disabled");
        var loader = form.find(".ajax-loader").show();
        var request = $.ajaxq("form-add", { url: url, type: method, data: data });
        request.done(function(resp) {
            if (resp.Errors) {
                var err = resp.Errors[0];
                Accounts.showError(form, err.ErrorMessage);
                return;
            }

            form.find(".errors").empty().hide();
            if ($.isFunction(success)) {
                success(resp);
            }
            if (hideModalOnSave !== false) {
                $(".modal.in").modal("hide");
            }
        });

        request.always(function() {
            loader.hide();
            submitButton.removeClass("disabled").removeAttr("disabled");
        });
    },

    initAddModals: function(success, hideModalOnSave) {
        var modalOpts = {
            show: false,
            keyboard: false
        };

        var personModal = $("#create-person-dialog").modal(modalOpts);
        var businessModal = $("#create-business-dialog").modal(modalOpts);

        Accounts.personDefaults = {
            info: null,
            action: personModal.find("form:first").attr("action"),
            method: personModal.find("form:first").attr("method"),
            data: null
        };

        Accounts.businessDefaults = {
            info: null,
            action: businessModal.find("form:first").attr("action"),
            method: businessModal.find("form:first").attr("method"),
            data: null
        };

        $(document).on("click", "input.copy-mailing", function() {
            var addresses = $(this).closest(".address-container").find(".address");
            if ($(this).is(":checked")) {
                addresses.filter(".Billing").find("input,select").addClass("disabled").attr("disabled", "disabled");
            } else {
                addresses.filter(".Billing").find("input,select").removeClass("disabled").removeAttr("disabled");
            }
        });

        $(document).on("click", "a.add-account", function() {
            var type = $(this).data("type");
            var accountjson = $(this).data("account-json") || {};
            Accounts.showModal(type, accountjson);
            return false;
        });

        var forms = [personModal.find("form"), businessModal.find("form")];
        _.each(forms, function(form) {
            initVerifyAddress({
                addressForm: form,
                verifyContainer: form.find(".verified-address-container"),
                unverifyContainer: form.find(".unverified-address-container"),
                submit: function() {
                    form.find(".ajax-loader").show();
                    form.find(".errors").empty().hide();
                },
                error: function(msg) {
                    Accounts.showError(form, msg);
                    form.find(".ajax-loader").hide();
                },
                success: function() {
                    form.find(".ajax-loader").hide();
                    var inputs = form.find(".address.Mailing").find("input,select");
                    var street1 = inputs.filter("[name=street1]").val();
                    var street2 = inputs.filter("[name=street2]").val();
                    var city = inputs.filter("[name=city]").val();
                    var state = inputs.filter("[name=state]").val();
                    var zip = inputs.filter("[name=zip]").val();

                    // address hasn't changed, no need to do verification
                    if (Accounts.prevMailing) {
                        if (street1 === Accounts.prevMailing.Street1 && street2 === Accounts.prevMailing.Street2 &&
                         city === Accounts.prevMailing.City && state === Accounts.prevMailing.State && zip === Accounts.prevMailing.Zip) {
                            form.find(".selected-address").find("input").attr("checked", "checked");
                            form.trigger("saveform");
                            return;
                        }
                    }

                    form.find(".page").hide();
                    form.find(".page2").show();
                    form.find(".search-results").show();
                }
            });
        });

        $(document).on("click", ".back", function() {
            var modal = $(this).closest(".modal-body");
            modal.find(".page").hide();
            modal.find(".page1").show();
        });

        $(document).on("click", "button.save-form", function() {
            $(this).closest("form").trigger("saveform");
            return false;
        });

        $(document).on("saveform", "form.add-account", function(e) {
            e.preventDefault();
            Accounts.saveForm($(this), success, hideModalOnSave);
        });
    },

    refreshAccount: function(accountid) {
        Accounts.fullCache = _.reject(Accounts.fullCache, function(a) {
            return a.AccountID === accountid;
        });
        Accounts.refreshMenu(false, accountid);
        if (Accounts.elements.searchBox) {
            Accounts.elements.searchBox.autosearch("clearCache");
        }
    },

    init: function(show) {
        Utility.initBase(Accounts);
        Accounts.elements.addCardModal.modal({ show: false });
        Accounts.elements.accountContainer.on("click", "#add-credit-card", function() {
            Accounts.elements.addCardModal.modal("show");
            return false;
        });
        Accounts.elements.addCardModal.find("form").submit(function() {
            var data = $(this).serializeObject();
            var accountid = $("#account-detail").data("accountid");
            data.accountid = accountid;

            $.post(Accounts.urls.addCard, data, function() {
                $(".modal.in").modal("hide");
                Accounts.refreshAccount(accountid);
            });

            return false;
        });

        $(".modal .cancel").click(function() {
            $(".modal.in").modal("hide");
            return false;
        });

        var addModal = $("#add-quote-modal").modal({ show: false });
        var modalSave = addModal.find(".save-quote");
        var modalNext = addModal.find(".next-modal");
        var dropdownOptions = {
            business: "--NEW BUSINESS--",
            person: "--NEW PERSON--",
            billing: "--SAME-AS-BILLING--"
            //existing: "--EXISTING ACCOUNT--", /// NOT SUPPORTED YET
        };

        addModal.on("shown", function() {
            addModal.find("#shippingaccountid").focus();
        });

        addModal.on("change", "#shippingaccountid", function() {
            var val = $(this).val();
            // we only "save" the modal from this screen if we're using the same as the billing account
            var isSave = (val === dropdownOptions.billing);
            modalSave.toggle(isSave);
            modalNext.toggle(!isSave);
        });

        addModal.find("form").on("submit", function(e) {
            var val = addModal.find("#shippingaccountid").val();
            var form = this;
            var $form = $(this);

            // for billing accounts, we just submit the form
            if (val === dropdownOptions.billing || !form.checkValidity()) {
                return true;
            }

            e.preventDefault();

            $(".modal.in").modal("hide");

            var modalType = (val === dropdownOptions.person) ? "Person" : "Business";
            var displayName = $("#same-as-billing-option").text();
            var modal = Accounts.showModal(modalType, {}, {
                data: $form.serializeObject(),
                action: $form.attr("action"),
                method: $form.attr("method") || "get",
                info: "Adding a quote to <strong>" + displayName + "</strong>"
            });

            // make sure when we close the second modal, we reset the first, otherwise it's very confusing
            modal.one("hidden", function() {
                form.reset();
            });
        });

        Accounts.elements.accountContainer.on("click", "#add-quote", function() {
            addModal.modal("show");
            return false;
        });

        var storageModal = $("#add-storage-modal").modal({ show: false });
        Accounts.elements.accountContainer.on("click", "#add-storage", function() {
            storageModal.modal("show");
        });

        // interactive element setup
        Accounts.initAddModals(function(resp) {
            if (resp.redirect) {
                window.location = resp.redirect;
            }

            Accounts.refreshAccount(resp.AccountID);
        });

        Accounts.elements.searchBox.autosearch(Accounts.elements.menuContainer, {
            serviceUrl: Accounts.urls.search,
            cache: false,
            onSuggest: function(items) {
                Accounts.currentPage = 0;
                Accounts.refreshMenu(items, "auto");
            }
        });

        /** Event Binding **/
        Accounts.elements.menuContainer.on("click", ".account", function() {
            var id = $(this).data("accountid");
            Accounts.showAccount(id);
            return false;
        });

        Accounts.elements.accountContainer.on("click", "#account-quote-table tr", function() {
            var url = $(this).find("td a").attr("href");
            if (url) {
                Utility.showOverlay();
                window.location = url;
            }
        });

        $("#account-menu-scroll").hover(function() {
            $(this).css("overflow", "auto");
        }, function() {
            $(this).css("overflow", "hidden");
        });

        $("#account-menu-scroll").bind("scroll", function(e) {
            var elem = $(e.currentTarget);
            if (elem[0].scrollHeight - elem.scrollTop() - 5 < elem.outerHeight()) {
                Accounts.currentPage = Accounts.currentPage + 1;
                Accounts.refreshMenu(false);
            }
        });

        show = show || "auto";
        Accounts.refreshMenu(false, show);

        // Used to detect initial (useless) popstate.
        // If history.state exists, assume browser isn't going to fire initial popstate.
        var popped = ('state' in window.history);
        var initialURL = location.href;
        History.Adapter.bind(window, "statechange", function() {
            var initialPop = !popped && location.href == initialURL;
            if (initialPop) {
                return;
            }

            var state = History.getState();
            Accounts.showAccount(state.data.accountid, false, true);
        });
    }
};
