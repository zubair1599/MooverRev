(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.Account = (function() {

    function Account(resp) {
      $.extend(this, resp);
      this.BillingAddress = new Address(resp.BillingAddress);
      this.MailingAddress = new Address(resp.MailingAddress);
      this.PrimaryPhone = resp.PrimaryPhone ? new PhoneNumber(resp.PrimaryPhone) : null;
      this.SecondaryPhone = resp.SecondaryPhone ? new PhoneNumber(resp.SecondaryPhone) : null;
      this.FaxPhone = resp.FaxPhone ? new PhoneNumber(resp.FaxPhone) : null;
    }

    return Account;

  })();

  this.PersonAccount = (function(_super) {

    __extends(PersonAccount, _super);

    function PersonAccount(json) {
      PersonAccount.__super__.constructor.call(this, json);
      this.FirstName = json.FirstName, this.LastName = json.LastName;
    }

    return PersonAccount;

  })(Account);

  this.BusinessAccount = (function(_super) {

    __extends(BusinessAccount, _super);

    function BusinessAccount(json) {
      BusinessAccount.__super__.constructor.call(this, json);
      this.Name = json.Name, this.BusinessType = json.BusinessType, this.ViewBusinessType = json.ViewBusinessType;
    }

    return BusinessAccount;

  })(Account);

  this.PhoneNumber = (function() {

    function PhoneNumber(json) {
      if (json) {
        $.extend(this, json);
      }
    }

    return PhoneNumber;

  })();

}).call(this);
