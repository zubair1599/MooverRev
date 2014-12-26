/// <reference path="references.js" />

/*global JoelPurra,Utility,prompt,Repository,Inventory,Keys, Kalendae*/
/*!
 * Copyright (c) 2012 Moovers Franchising, Inc.
 * Credits: http://crm.1800moovers.com/credits.txt
 * License: http://crm.1800moovers.com/license.txt
 */
/*!
    site.js
*/

/***

Some of the more common JS keycodes we use

****/
window.Keys = {
    TAB: 9,
    BACKSPACE: 8,
    ENTER: 13,
    CAPS_LOCK: 20,

    COMMAND: 91,
    COMMAND_LEFT: 91,
    WINDOWS: 91,
    COMMAND_RIGHT: 93,

    COMMA: 188,
    PERIOD: 190,

    SHIFT: 16,
    CONTROL: 17,
    ALT: 18,
    DELETE: 46,
    ESCAPE: 27,
    INSERT: 45,
    
    NUMPAD_MULTIPLY: 106,
    NUMPAD_ADD: 107,
    NUMPAD_ENTER: 108,
    NUMPAD_SUBTRACT: 109,
    NUMPAD_DECIMAL: 110,
    NUMPAD_DIVIDE: 111,
    
    SPACE: 32,
    PAGE_UP: 33,
    PAGE_DOWN: 34,
    END: 35,
    HOME: 36,
    LEFT: 37,
    UP: 38,
    RIGHT: 39,
    DOWN: 40,
    
    A: 65,
    S: 83,
    X: 88,
    Y: 89,
    Z: 90
};

/***
    Configure plugins
***/

// "Plus As Tab" allows users to post quotes using only the keypad
// configure plugin to use "Enter" instead of "Plus"
JoelPurra.PlusAsTab.setOptions({ key: window.Keys.ENTER });

// underscore templating configuration

// use {{- }} instead of <%- %>
_.templateSettings.escape = /\{\{-(.*?)\}\}/g;
// use {{= }} instead of <%= %>
_.templateSettings.interpolate = /\{\{=([\s\S]+?)\}\}/g;
// use {{ }} instead of <% %>
_.templateSettings.evaluate = /\{\{([\s\S]+?)\}\}/g;

/***
    Base object extensions
***/

// array.empty -- removes all objects from an array
Object.defineProperty(Array.prototype, "empty", {
    enumerable: false,
    value: function() {
        this.length = 0;
    }
});

// string.contains -- test if a string contains a substring
Object.defineProperty(String.prototype, "contains", {
    enumerable: false,
    value: function(str) {
        return this.indexOf(str) >= 0;
    }
});

// string.capitalize -- uppercase first letter of str
Object.defineProperty(String.prototype, "capitalize", {
    enumerable: false,
    value: function() {
        return this.charAt(0).toUpperCase() + this.slice(1);
    }
});

/***
    Underscore.js extensions
***/

(function() {
    var partial_cache = { };

    _.mixin({
        // returns the sum of the result of all iterators
        // if "iterator" is a string, returns the sum of that property
        sum: function(array, iterator) {
            var toSum = array;

            if (_.isString(iterator)) {
                toSum = _.pluck(array, iterator);
            } else if (_.isFunction(iterator)) {
                toSum = _.map(array, iterator);
            }

            if (_.size(array) === 0) {
                return 0;
            }

            return _.reduce(toSum, function(a, b) {
                return a + b;
            });
        },

        partial: function(template, data) {
            var templateID = "#" + template + "-partial";
            if (!_.contains(Object.keys(partial_cache), templateID)) {
                var html = $(templateID).html();
                partial_cache[templateID] = _.template(html);
            }

            return partial_cache[templateID](data);
        }
    });
})();

/*** jQuery object extensions ***/
(function($) {
    // serializes a form into key/value pair.
    $.fn.serializeObject = function() {
        var ret = {};
        $.each(this.serializeArray(), function() {
            var value = this.value;
            var key = this.name;
            var cur = ret[key];

            // ASP.NET sometimes creates a hidden field with the value of "false"
            // treat a "false" value as though it undefined
            var isnew = _.isUndefined(cur) || cur === "false";
            if (isnew) {
                // if key is undefined in the current object, add a key/value w/ the value
                ret[key] = value || "";
                return;
            }

            if (value === "false") {
                return;
            }

            // if there are multiple non-false values, create an array with all values
            if (!cur.push) {
                cur = ret[key] = [ret[key]];
            }

            cur.push(value || "");
        });

        return ret;
    };

    // shorthand function to get value of a textbox or text of an element
    $.fn.valOrText = function() {
        return this.val() || this.text();
    };

    // Toggle attribute (attributeName, condition, [ value ])
    $.fn.toggleAttr = function(attr, bit, val) {
        if (!val) {
            val = true;
        }

        if (_.isUndefined(bit)) {
            bit = !this.is("[" + attr + "]");
        }

        if (bit) {
            return this.attr(attr, val);
        }

        return this.removeAttr(attr);
    };

    // reset -- sets a DOM element back to it's "empty" value
    $.fn.reset = function() {
        return this.each(function() {
            var item = $(this);
            // for a select box, select the default value
            if (item.is("select")) {
                item.val(item.prop("defaultSelected"));
            }
            // for a form, call the native "reset" function
            else if (item.is("form")) {
                item[0].reset();
            }
            // for a checkbox, uncheck it
            else if (item.is(":checkbox")) {
                item.removeAttr("checked");
            }
            // for other inputs, set value to empty string
            else if (item.is(":input:not(:submit):not(:button)")) {
                item.val("");
            }
            else {
                return;
            }
        });
    };

    // returns a deferred for set timeout
    // $.wait(500).then(function() { /* executed after 500ms */ });
    $.wait = function(time) {
        return $.Deferred(function(dfd) {
            setTimeout(dfd.resolve, time);
        });
    };

    // add a "$.support.inputs" value for each of the html5 inputs.
    $.support.inputs = {};
    var item = document.createElement("input");
    $.each("search number range color tel url email date month week time datetime datetime-local".split(" "), function(idx, prop) {
        item.setAttribute("type", prop);
        $.support.inputs[prop] = (item.type === prop);
    });

})(jQuery);

/*ignore jslint start*/
// Full version of `log` that:
//  * Prevents errors on console methods when no console present.
//  * Exposes a global 'log' function that preserves line numbering and formatting.
(function() {
    var method;
    var noop = function() { };
    var methods = [
        'assert', 'clear', 'count', 'debug', 'dir', 'dirxml', 'error',
        'exception', 'group', 'groupCollapsed', 'groupEnd', 'info', 'log',
        'markTimeline', 'profile', 'profileEnd', 'table', 'time', 'timeEnd',
        'timeStamp', 'trace', 'warn'
    ];
    var length = methods.length;
    var console = (window.console = window.console || {});

    while (length--) {
        method = methods[length];

        // Only stub undefined methods.
        if (!console[method]) {
            console[method] = noop;
        }
    }

    if (Function.prototype.bind) {
        window.log = Function.prototype.bind.call(console.log, console);
    }
    else {
        window.log = function() {
            Function.prototype.apply.call(console.log, console, arguments);
        };
    }
})();
/*ignore jslint end*/

$(function() {
    // when we show a modal, by default, focus the first visible input
    $(".modal").on("shown", function() {
        $(this).find("input:visible:first").focus();
    });


    // add a "full-row-link" class that will make clicking on a table row the same as clicking on the link
    $("table").on("click touchstart", "tr", function(e) {
        if ($(e.target).closest("input,button,a").length > 0) {
            return true;
        }
        

        var url = $(this).find("a.full-row-link").attr("href");
        if (url) {
            window.location = url;
        }
    });

    // add a "mask" to credit card/telephone #s #s
    $("input[name=cardnumber]").mask("9999-9999-9999-99?99");
    $("input[type=tel]:not(.extension)").mask("(***)***-****");

    // create a "no-overflow" textarea class, where we don't allow additional input into a text area if it's already at it's maximum lines
    $("textarea.no-overflow").keydown(function(e) {
        var lines = $(this).val().split("\n").length;
        if (e.keyCode === window.Keys.ENTER && lines >= $(this).attr("rows")) {
            return false;
        }
    });


    // add a "data-popup" link attribute, which will open the link in a new popup window
    $("a[data-popup=true]").click(function() {
        var href = $(this).attr("href");
        var a = window.open(href, "", "height=500,width=795,location=1,menubar=0,scrollbars=1,status=0,titlebar=0,toolbar=0");
        $(a).find(".main-body").css("padding-top", 0);
        return false;
    });

    // create a heartbeat, so we can't accidentally do a bunch of stuff while the session times out
    (function heartbeat() {
        var FOUR_HOURS = 14400000;
        $.wait(FOUR_HOURS).then($.get(SERVER.baseUrl + "keepalive")).done(heartbeat);
    })();


    /*** Polyfills ***/
    if (!$.support.inputs.date) {
        // if we don't support <input type="date">, switch from ISO dates to US standard
        // then initialize a datepicker
        $("input[type=date]").each(function() {
            var input = this;
            var val = Kalendae.moment(input.value);
            if (val) {
                val = val.format("MM/DD/YYYY");
                input.value = val;
            } else {
                input.value = Kalendae.moment(new Date()).format("MM/DD/YYYY");
            }

            $(input).kalendae({
               format: "MM/DD/YYYY"
            });
        });
    }


    /*** Some application specific plugins that we use on several pages. ***/

    // on payments, show an alert when we remove messages
    $("table").on("click", ".payment-row .icon-remove", function() {
        var url = SERVER.baseUrl + "Payment/CancelPayment";
        var paymentid = $(this).closest(".payment-row").data("paymentid");
        var paymentMethod = $(this).closest(".payment-row").data("paymenttype");
        var amount = $(this).closest(".payment-row").data("amount");

        var msg = "";
        // removing a Credit Card payment does NOT actually cancel the payment it only removes the record of its
        if (paymentMethod.toLowerCase() === "creditcard") {
            msg += "IMPORTANT: This payment will have to be refunded through the FirstData console.\n\n";
        }
        msg += "Please enter a reason for removing this " + amount + " payment";
        var reason = prompt(msg);
        if (reason) {
            Utility.showOverlay();
            $.post(url, { paymentid: paymentid, reason: reason }, function() {
                window.location.reload();
            });
        }
    });

    // automatically create a "toggle panel for survey panels
    $(".survey-panel").togglePanel();

});

// Credit Card type Regexes -- only match with slashes stripped
window.CardTypes = {
    Visa: /^4[0-9]{12}(?:[0-9]{3})?$/,
    Mastercard: /^5[1-5][0-9]{14}$/,
    Amex: /^3[47][0-9]{13}$/,
    Discover: /^6(?:011|5[0-9]{2})[0-9]{12}$/,
    DinersClub: /^3(?:0[0-5]|[68][0-9])[0-9]{11}$/,
    JCB: /^(?:2131|1800|35\d{3})\d{11}$/
};

window.Utility = {
    lineBreakToBr: function(string) {
        return string.replace(/\n/g, "<br />");
    },
    htmlEncode: function(string) {
        return $("<div>").text(string).html();
    },
    htmlDecode: function(string) {
        return $("<div>").html(string).text();
    },
    roundTo: function(num, digits) {
        digits = digits || 0;
        return Math.round(num * Math.pow(10, digits)) / Math.pow(10, digits);
    },
    // get array of #s from "from" to "to"
    range: function(from, to, step) {
        from = parseFloat(from);
        to = parseFloat(to);
        step = parseFloat(step);
    
        if (isNaN(from)) {
            throw "From undefined";
        }
        if (isNaN(to)) {
            to = from;
        }
        if (isNaN(step)) {
            step = 1;
        }
        if (step === 0) {
            throw "Step must not be 0";
        }
        if (from > to && step > 0 || from < to && step < 0) {
            throw "Can't step backwards";
        }
    
        // # of significant digits to keep
        var decimals = 0;
        [ from, to, step ].forEach(function(i) {
            var thisDec = (i.toString().split(".")[1] || "").length;
            decimals = Math.max(thisDec, decimals);
        });

        var flip = false;
        if (step <= 0) {
            var tmp = from;
            from = to;
            to = tmp;
            step = -step;
            flip = true;
        }

        var roundTo = function(num, digits) {
            digits = digits || 0;
            return Math.round(num * Math.pow(10, digits)) / Math.pow(10, digits);
        };

        var cur = from;
        var ret = [];
        while (cur <= to) {
            ret.push(cur);
            cur = roundTo(step + cur, decimals);
        }

        if (flip) {
            ret = ret.reverse();
        }

        return ret;
    },
    roundCurrency: function(num) {
        return this.roundTo(num, 2);
    },
    // loop through all keys of an object, apply values to form elements of that name
    objectToForm: function(obj, form) {
        _.each(obj, function(val, key) {
            form.find("[name=" + key + "]").val(val);
        });
    },
    initBase: function(obj) {
        // example:
        // obj = {
        //      templates: {
        //          // "test" is a compiled underscore template of the html content in "#id-of-script"
        //          test : "#id-of-template"
        //
        //          // testWithOptions is a precompiled template using "options"
        //          testWithOptions: {
        //              element: "#id-of-template",
        //              options: { variable: "test", other: "options" } 
        //          }
        //      }
        // }
        if (obj.templates) {
            _.each(obj.templates, function(selector, key) {

                if ($.isPlainObject(selector) && selector.options) {
                    var html = $(selector.element).html();
                    obj.templates[key] = _.template(html, null, selector.options);
                } else {
                    obj.templates[key] = _.template($(selector).html());
                }
            });
        }
        // turns list of element selectors into actual jquery selectors
        if (obj.elements) {
            _.each(obj.elements, function(selector, key) {
                obj.elements[key] = $(selector);
            });
        }
        // turns a list of selectors into boostrap modals
        if (obj.dialogs) {
            _.each(obj.dialogs, function(selector, key) {
                obj.dialogs[key] = $(selector).modal({
                    show: false,
                    keyboard: false
                });
            });
        }
    },
    // get random string of 'length' characters - defaults to 12
    randomid: function(length) {
        length = length || 12;
        var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz".split("");
        var str = "";
        for (var i = 0; i < length; i++) {
            str += chars[Math.floor(Math.random() * chars.length)];
        }
        return str;
    },
    // replace the first item matching comparer in array with "replaceWith"
    replace: function(array, replaceWith, comparer) {
        var index = _.indexOf(array, _.find(array, comparer));
        if (index >= 0) {
            array[index] = replaceWith;
        }
    },
    // turn "TRUE", "true", "TrUe", etc into the boolean "true"
    parseBool: function(str) {
        if (typeof(str) === "boolean") {
            return str;
        }
        if (typeof (str) === "string" || str instanceof String) {
            return str.toLowerCase() === "true";
        }

        return false;
    },
    isJson: function(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    },

    /** "Overlay" covers the whole screen, is transparent, makes all elements unclickable, and make the cursor have a "loading" icon */
    hideOverlay: function() {
        $("#overlay").hide();
    },
    showOverlay: function() {
        $("#overlay").show();
    },

    // Luhn check on CC#, then run it across commonly accepted credit card regexes
    isValidCreditCard: function(number) {
        function checkLuhn(input) {
            var sum = 0;
            var numdigits = input.length;
            var parity = numdigits % 2;
            for (var i = 0; i < numdigits; i++) {
                var digit = parseInt(input.charAt(i), 10);
                if (i % 2 === parity) {
                    digit *= 2;
                }
                if (digit > 9) {
                    digit -= 9;
                }
                sum += digit;
            }
            return (sum % 10) === 0;
        }

        if (!checkLuhn(number)) {
            return false;
        }

        return _.any(window.CardTypes, function(regex) {
            if (number.match(regex)) {
                return true;
            }
        });
    },
    // returns a time estimate in hours
    getEstimateString: function(time) {
        var hours = Math.floor(time / 60);

        // < 1 hour = 30 minute range
        if (hours === 0) {
            return "30 - 60 Minutes";
        }
        // < 4 hours = 1 hour range
        else if (hours < 4) {
            return Math.ceil(hours) + " - " + Math.ceil(hours + 1) + " Hours";
        }
        // < 12 hours = 2 hour range
        if (hours < 12) {
            return Math.ceil(hours) + " - " + Math.ceil(hours + 2) + " Hours";
        }
        // > 12 hours = 4 hour range
        return Math.ceil(hours) + " - " + Math.ceil(hours + 4) + " Hours";
    },

    /*
        Format "minutes" as hours, to "accuracy"

        On "accuracy":
        Default = 1 = 1 minute; 15 = 15 minutes, etc
    */
    formatHours: function(totalMinutes, accuracy, isShort) {
        if (_.isUndefined(isShort)) {
            isShort = accuracy;
            accuracy = 1;
        }

        totalMinutes = Math.round(totalMinutes);

        var hours = Math.floor(totalMinutes / 60);
        var minutes = Math.floor(totalMinutes % 60);

        minutes = Math.ceil(minutes / accuracy) * accuracy;
        if (minutes === 60) {
            minutes = 0;
            hours = hours + 1;
        }

        if (isShort) {
            return hours + ":" + ((minutes < 10) ? "0" : "") + minutes;
        }

        if (hours === 0) {
            return minutes + " Minutes";
        }

        return hours + " Hours, " + minutes + " Minutes";
    },
    formatCurrency: function(number, symbol, thousands_sep, decimals, decimal_sep) {
        ///<summary>
        /// Format a number as currency
        /// Based on: http://stackoverflow.com/questions/149055/how-can-i-format-numbers-as-money-in-javascript
        /// Cleaned up, modifications © 2013 Moovers Franchising
        ///</summary>
        decimals = isNaN(decimals) ? 2 : Math.abs(decimals);
        decimal_sep = (typeof (decimal_sep) === "undefined") ? "." : decimal_sep;
        thousands_sep = (typeof (thousands_sep) === "undefined") ? "," : thousands_sep;
        symbol = (typeof (symbol) === "undefined") ? "$" : symbol;

        var sign = (number < 0) ? "-" : "";

        // positive integer part of number cast as a string
        var intval = parseInt(Math.abs(number), 10).toString();

        // length of the first "thousands" set
        var j = (intval.length > 3) ? intval.length % 3 : 0;

        var ret = sign + symbol;
        if (j > 0) {
            ret += intval.substr(0, j) + thousands_sep;
        }

        // uses a lookahead to find groups of 3 digit numbers with a digit afterwards
        // http://www.regular-expressions.info/lookaround.html
        var regex = /(\d{3})(?=\d)/g;
        ret += intval.substr(j).replace(regex, "$1" + thousands_sep);

        if (decimals > 0) {
            ret += decimal_sep + Math.abs(Math.abs(number) - intval).toFixed(decimals).slice(2);
        }

        return ret;
    },
    // turn "$20,000.01 into a float -- only works on American $
    parseCurrency: function(str) {
        return parseFloat($.trim(str).replace("$", "").replace(",", ""));
    },
    addCustomLinks: function(text) {
        var tmp = $("<div>");
        var accountRegex = /(a\d+)/gi;
        var quoteRegex = /quote (\d+)/gi;
        var emailRegex = /[_\-a-z\d]+@[_\-a-z\d+\.]+/gi;

        // replace probably accounts, quotes, and e-mail addresses w/ links
        tmp.html(text.replace(accountRegex, function(str, accountid) {
            return "<a href=\"" + SERVER.baseUrl + "Accounts/Index/" + accountid + "\">" + str + "</a>";
        }).replace(quoteRegex, function(str, quoteid) {
            return "<a href=\"" + SERVER.baseUrl + "Quote/Overview/" + quoteid + "\">" + str + "</a>";
        }).replace(emailRegex, function(str) {
            return "<a href=\"mailto:" + str + "\">" + str + "</a>";
        }));

        return tmp.html();
    }
};

window.SearchFunctions = {
    vehicle: function(vehicle) {
        return function(v) {
            return v.VehicleID === vehicle.VehicleID;
        };
    },
    employee: function(emp) {
        return function(e) {
            return e.EmployeeID === emp.EmployeeID;
        };
    },
    item: function(item) {
        return function(i) {
            return i.ItemID === item.ItemID;
        };
    },

    /**
        Vehicles, Employees, and Items all have "keycodes" -- we need to have custom AutoComplete sorts for these items
        see "Autocomplete" options, "orderby"
    **/
    itemAutocompleteSort: function(query) {
        return function(a, b) {
            var aStart = a.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var bStart = b.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var itemA = Inventory.getItem(a);
            var itemB = Inventory.getItem(b);

            // if keycode exactly matches, sort those ahead of everything
            if (itemA && itemA.KeyCode === +query) {
                return -1;
            }
            if (itemB && itemB.KeyCode === +query) {
                return 1;
            }

            // sort items matching at the beginning of the word before others
            if (aStart && !bStart) {
                return -1;
            }
            if (bStart && !aStart) {
                return 1;
            }

            // finally, sort alphabetically
            return (a > b) ? 1 : -1;
        };
    },
    vehicleAutocompleteSort: function(query) {
        return function(a, b) {
            var aStart = a.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var bStart = b.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var itemA = Repository.getVehicle(a);
            var itemB = Repository.getVehicle(b);

            // if keycode exactly matches, sort those ahead of everything
            if (itemA && itemA.Lookup === query) {
                return -1;
            }
            if (itemB && itemB.Lookup === query) {
                return 1;
            }

            // sort items matching at the beginning of the word before others
            if (aStart && !bStart) {
                return -1;
            }
            if (bStart && !aStart) {
                return 1;
            }

            // finally, sort alphabetically
            return (a > b) ? 1 : -1;
        };
    },
    employeeAutocompleteSort: function(query) {
        return function(a, b) {
            var aStart = a.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var bStart = b.toLowerCase().indexOf(query.toLowerCase()) === 0;
            var itemA = Repository.getEmployee(a);
            var itemB = Repository.getEmployee(b);

            // if keycode exactly matches, sort those ahead of everything
            if (itemA && itemA.Lookup === query) {
                return -1;
            }
            if (itemB && itemB.Lookup === query) {
                return 1;
            }

            // sort items matching at the beginning of the word before others
            if (aStart && !bStart) {
                return -1;
            }
            if (bStart && !aStart) {
                return 1;
            }

            // finally, sort alphabetically
            return (a > b) ? 1 : -1;
        };
    }
};

// Approx. state from zip code ranges
window.Utility.ZipCodeLookups = [
    { value: "AK", min: "99501", max: "99950" },
    { value: "AL", min: "35004", max: "36925" },
    { value: "AR", min: "71601", max: "72959" },
    { value: "AR", min: "75502", max: "75502" },
    { value: "AZ", min: "85001", max: "86556" },
    { value: "CA", min: "90001", max: "96162" },
    { value: "CO", min: "80001", max: "81658" },
    { value: "CT", min: "06001", max: "06389" },
    { value: "CT", min: "06401", max: "06928" },
    { value: "DC", min: "20001", max: "20039" },
    { value: "DC", min: "20042", max: "20599" },
    { value: "DC", min: "20799", max: "20799" },
    { value: "DE", min: "19701", max: "19980" },
    { value: "FL", min: "32004", max: "34997" },
    { value: "GA", min: "30001", max: "31999" },
    { value: "GA", min: "39901", max: "39901" },
    { value: "HI", min: "96701", max: "96898" },
    { value: "IA", min: "50001", max: "52809" },
    { value: "IA", min: "68119", max: "68120" },
    { value: "ID", min: "83201", max: "83876" },
    { value: "IL", min: "60001", max: "62999" },
    { value: "IN", min: "46001", max: "47997" },
    { value: "KS", min: "66002", max: "67954" },
    { value: "KY", min: "40003", max: "42788" },
    { value: "LA", min: "70001", max: "71232" },
    { value: "LA", min: "71234", max: "71497" },
    { value: "MA", min: "01001", max: "02791" },
    { value: "MA", min: "05501", max: "05544" },
    { value: "MD", min: "20331", max: "20331" },
    { value: "MD", min: "20335", max: "20797" },
    { value: "MD", min: "20812", max: "21930" },
    { value: "ME", min: "03901", max: "04992" },
    { value: "MI", min: "48001", max: "49971" },
    { value: "MN", min: "55001", max: "56763" },
    { value: "MO", min: "63001", max: "65899" },
    { value: "MS", min: "38601", max: "39776" },
    { value: "MS", min: "71233", max: "71233" },
    { value: "MT", min: "59001", max: "59937" },
    { value: "NC", min: "27006", max: "28909" },
    { value: "ND", min: "58001", max: "58856" },
    { value: "NE", min: "68001", max: "68118" },
    { value: "NE", min: "68122", max: "69367" },
    { value: "NH", min: "03031", max: "03897" },
    { value: "NJ", min: "07001", max: "08989" },
    { value: "NM", min: "87001", max: "88441" },
    { value: "NV", min: "88901", max: "89883" },
    { value: "NY", min: "06390", max: "06390" },
    { value: "NY", min: "10001", max: "14975" },
    { value: "OH", min: "43001", max: "45999" },
    { value: "OK", min: "73001", max: "73199" },
    { value: "OK", min: "73401", max: "74966" },
    { value: "OR", min: "97001", max: "97920" },
    { value: "PA", min: "15001", max: "19640" },
    { value: "RI", min: "02801", max: "02940" },
    { value: "SC", min: "29001", max: "29948" },
    { value: "SD", min: "57001", max: "57799" },
    { value: "TN", min: "37010", max: "38589" },
    { value: "TX", min: "73301", max: "73301" },
    { value: "TX", min: "75001", max: "75501" },
    { value: "TX", min: "75503", max: "79999" },
    { value: "TX", min: "88510", max: "88589" },
    { value: "UT", min: "84001", max: "84784" },
    { value: "VA", min: "20040", max: "20041" },
    { value: "VA", min: "20040", max: "20167" },
    { value: "VA", min: "20042", max: "20042" },
    { value: "VA", min: "22001", max: "24658" },
    { value: "VT", min: "05001", max: "05495" },
    { value: "VT", min: "05601", max: "05907" },
    { value: "WA", min: "98001", max: "99403" },
    { value: "WI", min: "53001", max: "54990" },
    { value: "WV", min: "24701", max: "26886" },
    { value: "WY", min: "82001", max: "83128" }
];

window.Utility.findStateFromZip = function(zip) {
    var tester = parseInt(zip, 10);
    var zc = _.find(window.Utility.ZipCodeLookups, function(i) {
        var min = parseInt(i.min, 10);
        var max = parseInt(i.max, 10);
        if (tester >= min && tester <= max) {
            return true;
        }

        return false;
    });

    if (zc) {
        return zc.value;
    }

    return "";
};

// ↑ ↑ ↓ ↓ ← → ← → will hide prices from the quoting page -- often customers come in, and it's very awkward to have the "% discount" shown on the quoting page.
$(function() {
    var magic = [ Keys.UP, Keys.UP, Keys.DOWN, Keys.DOWN, Keys.LEFT, Keys.RIGHT, Keys.LEFT, Keys.RIGHT ];
    var input = [];
    $(document).on("keydown", function(e) {
        input.push(e.keyCode);
        if (input.length > magic.length) {
            input.shift();
        }

        // whether or not to show is storn in a cookie -- partial contra code toggles
        if (input.join() === magic.join()) {
            $.cookie("hideprice", !Utility.priceHidden(), { path: "/" });
        }
    });

    window.Utility.priceHidden = function() {
        return Utility.parseBool($.cookie("hideprice"));
    };
});