/*!
* @license PlusAsTab, EmulateTab
* Copyright (c) 2011, 2012 The Swedish Post and Telecom Authority (PTS)
* Developed for PTS by Joel Purra <http://joelpurra.se/>
* Released under the BSD license.
*/

/*
* @license EmulateTab
* Copyright (c) 2011, 2012 The Swedish Post and Telecom Authority (PTS)
* Developed for PTS by Joel Purra <http://joelpurra.se/>
* Released under the BSD license.
*
* A jQuery plugin to emulate tabbing between elements on a page.
*/

/*jslint vars: true, white: true, browser: true*/
/*global jQuery*/

// Set up namespace, if needed
var JoelPurra = JoelPurra || {};

(function($, namespace) {

    namespace.EmulateTab = function() {
    };

    var eventNamespace = ".EmulateTab";

    var focusable = ":input, a[href]";

    // Keep a reference to the last focused element, use as a last resort.
    var lastFocusedElement = null;

    // Private methods
    {
        function escapeSelectorName(str) {

            // Based on http://api.jquery.com/category/selectors/
            // Still untested
            return str.replace(/(!"#$%&'\(\)\*\+,\.\/:;<=>\?@\[\]^`\{\|\}~)/g, "\\\\$1");
        }

        function findNextFocusable($from, offset) {

            var $focusable = $(focusable)
                        .not(":disabled")
                        .not(":hidden")
                        .not("a[href]:empty");

            if ($from[0].tagName === "INPUT"
                && $from[0].type === "radio"
                && $from[0].name !== "") {

                var name = escapeSelectorName($from[0].name);

                $focusable = $focusable
                        .not("input[type=radio][name=" + name + "]")
                        .add($from);
            }

            var tabIndex = $from.attr("tabindex");
            if (typeof (tabIndex) !== "undefined") {

                tabIndex = parseInt(tabIndex, 10);

                var $withIndex = $(focusable).filter(":visible[tabindex]");

                var nextIdx = Infinity;

                var prevIdx = -Infinity;

                var $nextEl = $([]);

                var desc = offset < 0;

                $withIndex.each(function() {

                    var idx = parseInt($(this).attr("tabindex"), 10);

                    if (idx < nextIdx && idx > tabIndex || (desc && idx > prevIdx && idx < tabIndex)) {

                        $nextEl = $(this);

                        nextIdx = prevIdx = idx;
                    }
                })

                if ($nextEl.length) {
                    return $nextEl;
                }
            }

            var currentIndex = $focusable.index($from);

            var nextIndex = (currentIndex + offset) % $focusable.length;

            if (nextIndex <= -1) {

                nextIndex = $focusable.length + nextIndex;
            }

            var $next = $focusable.eq(nextIndex);

            return $next;
        }

        function focusInElement(event) {

            lastFocusedElement = event.target;
        }

        function tryGetElementAsNonEmptyJQueryObject(selector) {

            try {

                var $element = $(selector);

                if (!!$element
                    && $element.size() !== 0) {

                    return $element;
                }

            } catch (e) {

                // Could not use element. Do nothing.
            }

            return null;
        }

        // Fix for EmulateTab Issue #2
        // https://github.com/joelpurra/emulatetab/issues/2
        // Combined function to get the focused element, trying as long as possible.
        // Extra work done trying to avoid problems with security features around
        // <input type="file" /> in Firefox (tested using 10.0.1).
        // http://stackoverflow.com/questions/9301310/focus-returns-no-element-for-input-type-file-in-firefox
        // Problem: http://jsfiddle.net/joelpurra/bzsv7/
        // Fixed:   http://jsfiddle.net/joelpurra/bzsv7/2/
        function getFocusedElement() {

            var $focused = null

                // Try the well-known, recommended method first.
                || tryGetElementAsNonEmptyJQueryObject(':focus')

                // Fall back to a fast method that might fail.
                // Known to fail for Firefox (tested using 10.0.1) with
                // Permission denied to access property 'nodeType'.
                || tryGetElementAsNonEmptyJQueryObject(document.activeElement)

                // As a last resort, use the last known focused element.
                // Has not been tested enough to be sure it works as expected
                // in all browsers and scenarios.
                || tryGetElementAsNonEmptyJQueryObject(lastFocusedElement)

                // Empty fallback
                || $();

            return $focused;
        }

        function emulateTabbing($from, offset) {

            var $next = findNextFocusable($from, offset);

            $next.focus();
        }

        function initializeAtLoad() {

            // Start listener that keep track of the last focused element.
            $(document)
                .on("focusin" + eventNamespace, focusInElement);
        }
    }

    // Public functions
    {
        namespace.EmulateTab.forwardTab = function($from) {

            return namespace.EmulateTab.tab($from, +1);
        };

        namespace.EmulateTab.reverseTab = function($from) {

            return namespace.EmulateTab.tab($from, -1);
        };

        namespace.EmulateTab.tab = function($from, offset) {

            // Tab from focused element with offset, .tab(-1)
            if ($.isNumeric($from)) {

                offset = $from;
                $from = undefined;
            }

            $from = $from || namespace.EmulateTab.getFocused();

            offset = offset || +1;

            emulateTabbing($from, offset);
        };

        namespace.EmulateTab.getFocused = function() {

            return getFocusedElement();
        };

        $.extend({
            emulateTab: function($from, offset) {

                return namespace.EmulateTab.tab($from, offset);
            }
        });

        $.fn.extend({
            emulateTab: function(offset) {

                return namespace.EmulateTab.tab(this, offset);
            }
        });
    }

    // EmulateTab initializes listener(s) when jQuery is ready
    $(initializeAtLoad);

}(jQuery, JoelPurra));


/*
* @license PlusAsTab
* Copyright (c) 2011, 2012 The Swedish Post and Telecom Authority (PTS)
* Developed for PTS by Joel Purra <http://joelpurra.se/>
* Released under the BSD license.
*
* A jQuery plugin to use the numpad plus key as a tab key equivalent.
*/

/*jslint vars: true, white: true, browser: true*/
/*global jQuery*/

// Set up namespace, if needed
var JoelPurra = JoelPurra || {};

(function($, namespace) {
    namespace.PlusAsTab = {};

    var eventNamespace = ".PlusAsTab";

    // Keys from
    // http://api.jquery.com/event.which/
    // https://developer.mozilla.org/en/DOM/KeyboardEvent#Virtual_key_codes
    var KEY_NUM_PLUS = 107;

    // Add options defaults here
    var internalDefaults = {
        key: KEY_NUM_PLUS
    };

    var options = $.extend(true, {}, internalDefaults);

    var enablePlusAsTab = ".plus-as-tab, [data-plus-as-tab=true]";
    var disablePlusAsTab = ".disable-plus-as-tab, [data-plus-as-tab=false]";

    // Private functions
    {
        function performEmulatedTabbing(isTab, isReverse, $target) {

            isTab = (isTab === true);
            isReverse = (isReverse === true);

            if (isTab
                && $target !== undefined
                && $target.length !== 0) {

                $target.emulateTab(isReverse ? -1 : +1);

                return true;
            }

            return false;
        }

        function isChosenTabkey(key) {
            if (key === options.key
                || ($.isArray(options.key)
                    && $.inArray(key, options.key) !== -1)) {

                return true;
            }

            return false;
        }

        function isEmulatedTabkey(event) {
            // Checked later for reverse tab
            //&& !event.shiftKey

            if (!event.altKey
                && !event.ctrlKey
                && !event.metaKey
                && isChosenTabkey(event.which)) {

                return true;
            }

            return false;
        }

        function checkEmulatedTabKeyDown(event) {

            if (!isEmulatedTabkey(event)) {

                return;
            }

            var $target = $(event.target);

            if ($target.is(disablePlusAsTab)
                || $target.parents(disablePlusAsTab).length > 0
                || (!$target.is(enablePlusAsTab)
                    && $target.parents(enablePlusAsTab).length === 0)) {

                return;
            }

            var wasDone = performEmulatedTabbing(true, event.shiftKey, $target);

            if (wasDone) {

                event.preventDefault();
                event.stopPropagation();
                event.stopImmediatePropagation();

                return false;
            }

            return;
        }

        function initializeAtLoad() {

            $(document).on("keydown" + eventNamespace, checkEmulatedTabKeyDown);
        }
    }

    // Public functions
    {
        namespace.PlusAsTab.setOptions = function(userOptions) {

            // Merge the options onto the current options (usually the default values)
            $.extend(true, options, userOptions);
        };

        namespace.PlusAsTab.plusAsTab = function($elements, enable) {

            enable = (enable === undefined ? true : enable === true);

            return $elements.each(function() {

                var $this = $(this);

                $this
                    .not(disablePlusAsTab)
                    .not(enablePlusAsTab)
                    .attr("data-plus-as-tab", enable ? "true" : "false");
            });
        };

        $.fn.extend({
            plusAsTab: function(enable) {

                return namespace.PlusAsTab.plusAsTab(this, enable);
            }
        });
    }

    // PlusAsTab initializes listeners when jQuery is ready
    $(initializeAtLoad);

}(jQuery, JoelPurra));