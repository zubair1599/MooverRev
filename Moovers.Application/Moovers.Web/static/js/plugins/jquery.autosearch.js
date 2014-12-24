/*!
*  Ajax Autocomplete for jQuery, version 1.1.5
*  (c) 2010 Tomas Kirda, Vytautas Pranskunas
*
*  Ajax Autocomplete for jQuery is freely distributable under the terms of an MIT-style license.
*  For details, see the web site: http://www.devbridge.com/projects/autocomplete/jquery/
*
*  Last Review: 07/24/2012
*  NOTE: Modified by Aaron Marasco for use within the moovers CRM Application
*/

/*jslint onevar: true, evil: true, nomen: true, eqeqeq: true, bitwise: true, regexp: true, newcap: true, immed: true */
/*clearInterval: true, setInterval: true, jQuery: true*/
(function($) {
    function Autosearch(el, container, options) {
        this.el = $(el);
        this.el.attr('autosearch', 'off');
        this.container = $(container);
        this.suggestions = [];
        this.badQueries = [];
        this.currentValue = this.el.val();

        this.intervalId = 0;
        this.cachedResponse = [];
        this.onChange = null;
        this.ignoreValueChange = false;
        this.serviceUrl = options.serviceUrl;
        this.isLocal = false;
        this.options = {
            minChars: 4,
            deferRequestBy: 100,
            params: {},
            delimiter: null,
            onSuggest: $.noop,
            cache: true
        };

        this.initialize();
        this.setOptions(options);
        this.el.data('autosearch', this);
    }

    $.fn.autosearch = function(container, options) {
        var autosearchControl;

        if (typeof container == 'string') {
            var optionName = container;
            var option = options;

            autosearchControl = this.data('autosearch');
            if (typeof autosearchControl[optionName] == 'function') {
                autosearchControl[optionName](option);
            }
        } else {
            autosearchControl = new Autosearch(this.get(0) || $('<input />'), container, options);
        }

        return autosearchControl;
    };

    Autosearch.prototype = {
        initialize: function() {
            var me = this;
            this.el.keydown(function(e) { me.onKeyPress(e); });
            this.el.keyup(function(e) { me.onKeyUp(e); });
            this.el[0].onsearch = function(e) {
                me.onKeyUp(e);
            };
        },

        extendOptions: function(options) {
            $.extend(this.options, options);
        },

        setOptions: function(options) {
            var o = this.options;
            this.extendOptions(options);
            if (o.lookup || o.isLocal) {
                this.isLocal = true;
                if ($.isArray(o.lookup)) { o.lookup = { suggestions: o.lookup, data: [] }; }
            }
        },

        clearCache: function() {
            this.cachedResponse = [];
            this.badQueries = [];
        },

        onKeyPress: function(e) {
            if (this.disabled || !this.enabled) { return; }
            // return will exit the function
            // and event will not be prevented
            switch (e.keyCode) {
                case 27: //KEY_ESC:
                    this.el.val(this.currentValue);
                    this.hide();
                    break;
                case 9: //KEY_TAB:
                case 13: //KEY_RETURN:
                    return;
                default:
                    return;
            }
            //e.stopImmediatePropagation();
            //e.preventDefault();
        },

        onKeyUp: function(e) {
            if (this.disabled) { return; }
            switch (e.keyCode) {
                case 38: //KEY_UP:
                case 40: //KEY_DOWN:
                    return;
            }
            clearInterval(this.onChangeInterval);
            if (this.currentValue !== this.el.val()) {
                if (this.options.deferRequestBy > 0) {
                    // Defer lookup in case when value changes very quickly:
                    var me = this;
                    this.onChangeInterval = setInterval(function() { me.onValueChange(); }, this.options.deferRequestBy);
                } else {
                    this.onValueChange();
                }
            }
        },

        onValueChange: function() {
            clearInterval(this.onChangeInterval);
            this.currentValue = this.el.val();
            var q = this.getQuery(this.currentValue);
            this.selectedIndex = -1;
            if (this.ignoreValueChange) {
                this.ignoreValueChange = false;
                return;
            }

            this.getSuggestions(q);
        },

        getQuery: function(val) {
            var d, arr;
            d = this.options.delimiter;
            if (!d) { return $.trim(val); }
            arr = val.split(d);
            return $.trim(arr[arr.length - 1]);
        },

        getSuggestionsLocal: function(q) {
            var ret, arr, len, val, i;
            arr = this.options.lookup;
            len = arr.suggestions.length;
            ret = { suggestions: [], data: [] };
            q = q.toLowerCase();
            for (i = 0; i < len; i++) {
                val = arr.suggestions[i];
                if (val.toLowerCase().indexOf(q) === 0) {
                    ret.suggestions.push(val);
                    ret.data.push(arr.data[i]);
                }
            }
            return ret;
        },

        getSuggestions: function(q) {
            var cr = this.isLocal ? this.getSuggestionsLocal(q) : this.cachedResponse[q];
            var me = this;

            if (cr && $.isArray(cr)) {
                this.suggestions = cr;
                this.suggest();
            } else if (!this.isBadQuery(q)) {
                me = this;
                me.options.params.q = q;
                if (!me.options.cache) {
                    me.options.params._ = $.now();
                }

                $.get(this.serviceUrl, me.options.params, function(txt) { me.processResponse(q, txt); }, 'text');
            }
        },

        isBadQuery: function(q) {
            var i = this.badQueries.length;
            while (i--) {
                if (q.indexOf(this.badQueries[i]) === 0) { return true; }
            }
            return false;
        },

        suggest: function() {
            this.options.onSuggest(this.suggestions);
        },

        processResponse: function(query, text) {
            var response;
            try {
                response = eval('(' + text + ')');
            } catch (err) { return; }
            if (!$.isArray(response.data)) { response.data = []; }
            this.cachedResponse[query] = response;
            if (response.length == 0) {
                this.badQueries.push(query);
            }

            if (query === this.getQuery(this.currentValue)) {
                this.suggestions = response;
                this.suggest();
            }
        }
    };
}(jQuery));