quoteApp.factory('utilityFactory', ['$location',utilityFactory]);

function utilityFactory($location) {
    
    var serviceDefer = new Object();
    var protpcol = $location.$$protocol + '://';
    var host = $location.host();
    var port = ':' + $location.port();


    var url = protpcol + host + port;


    serviceDefer.URL = url;


    serviceDefer.formatCurrency = function (number, symbol, thousands_sep, decimals, decimal_sep) {
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

    };



    serviceDefer.getEstimateString= function(time) {
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
    };






    serviceDefer.formatHours = function(totalMinutes, accuracy, isShort) {
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
    

    };


    return serviceDefer;

};