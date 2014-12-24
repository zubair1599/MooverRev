/// <reference path="references.js" />
/*!
    addressVerify.js
*/

/*global Utility*/
(function(exports) {
    var Address = function(street1, street2, city, state, zip, addressID) {
        // from JSON
        if (Utility.isJson(street1)) {
            var json = JSON.parse(street1);
            street1 = json;
        }
        if ($.isPlainObject(street1)) {
            var obj = street1;
            street1 = obj.street1 || obj.Street1;
            street2 = obj.street2 || obj.Street2;
            city = obj.city || obj.City;
            state = obj.state || obj.State;
            zip = obj.zip || obj.Zip;
            addressID = obj.addressID || obj.AddressID;
        }

        this.Street1 = street1;
        this.Street2 = street2;
        this.City = city;
        this.State = state;
        this.Zip = zip;
        this.AddressID = addressID;
    };

    Address.prototype = $.extend(Address.prototype, {
        getCityState: function() {
            if (this.City && this.State) {
                return this.City + ", " + this.State;
            }
            if (this.City) {
                return this.City;
            }
            if (this.State) {
                return this.State;
            }

            return "";
        },
        display: function() {
            if (!this.Street1) {
                return this.getCityState() + " " + this.Zip;
            }

            if (this.Street2) {
                return this.Street1 + " " + this.Street2 + ", " + this.getCityState() + " " + this.Zip;
            }

            return this.Street1 + " " + this.getCityState() + " " + this.Zip;
        },
        toForm: function(form) {
            form.find("[name=street1]").val(this.Street1);
            form.find("[name=street2]").val(this.Street2);
            form.find("[name=city]").val(this.City);
            form.find("[name=state]").val(this.State);
            form.find("[name=zip]").val(this.Zip);
        }
    });

    var defaultOpts = {
        addressForm: $([]),
        verifyContainer: $([]),
        unverifyContainer: $([]),
        template: _.template($("#verify-address-template").html()),
        submit: $.noop,
        success: $.noop,
        error: $.noop,
        always: $.noop,
        verifyUrl: SERVER.baseUrl + "Address/GetSuggestions",
        verifyZip: SERVER.baseUrl + "Address/VerifyZipCode"
    };

    var initVerifyAddress = function(opts) {
        opts = $.extend({}, defaultOpts, opts);
        var addressForm = $(opts.addressForm);
        var verifyContainer = $(opts.verifyContainer);
        var unverifyContainer = $(opts.unverifyContainer);
        var success = opts.success;
        var error = opts.error;
        var template = opts.template;
        var verifyUrl = opts.verifyUrl;
        var verifyZip = opts.verifyZip;

        addressForm.on("submit", function(e) {
            var form = $(this);
            var address = form.serializeObject();

            if ($.isArray(address.street1)) {
                address.street1 = address.street1[0];
                address.street2 = address.street2[0];
                address.city = address.city[0];
                address.state = address.state[0];
                address.zip = address.zip[0];
            }

            e.preventDefault();

            if (!address.zip) {
                error("Zip Code is required for all addresses");
                return false;
            }

            opts.submit();
            $.postq("verify-address", verifyUrl, address, function(resp) {
                var verifiedaddresses = _.map(resp, function(verified) {
                    return {
                        verified: true,
                        json: JSON.stringify(verified),
                        displayString: function() {
                            return verified.delivery_line_1 + ", " + verified.last_line;
                        }
                    };
                });

                verifyContainer.empty();
                unverifyContainer.empty();

                verifyContainer.append(_.map(verifiedaddresses, function(addr) {
                    return template(addr);
                }).join());

                var showThisAddr = function() {
                    var thisaddr = {
                        verified: false,
                        json: JSON.stringify(address),
                        displayString: function() {
                            return new Address(address).display();
                        }
                    };

                    unverifyContainer.append(template(thisaddr));
                    success();
                };

                if (verifiedaddresses.length === 0) {
                    verifyContainer.append("<div class='alert alert-error'>Unable to verify address</div>");
                    $.post(verifyZip, { zip: address.zip }, function(resp) {
                        if (resp.Errors) {
                            error("Please ensure the zip code you entered is valid. If you're sure, please contact support.");
                        }
                        else {
                            showThisAddr();
                        }
                    });
                }
                else {
                    showThisAddr();
                }


            });
        });
    };

    exports.Address = Address;
    exports.initVerifyAddress = initVerifyAddress;
}(window));