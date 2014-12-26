class @Account
    constructor: (resp) -> 
        $.extend(this, resp);
        @BillingAddress = new Address(resp.BillingAddress);
        @MailingAddress = new Address(resp.MailingAddress);
        @PrimaryPhone = if resp.PrimaryPhone then new PhoneNumber(resp.PrimaryPhone) else null;
        @SecondaryPhone = if resp.SecondaryPhone then new PhoneNumber(resp.SecondaryPhone) else null;
        @FaxPhone = if resp.FaxPhone then new PhoneNumber(resp.FaxPhone) else null;

class @PersonAccount extends Account
    constructor: (json) ->
        super(json);
        { @FirstName, @LastName } = json;

class @BusinessAccount extends Account
    constructor: (json) ->
        super(json);
        { @Name, @BusinessType, @ViewBusinessType } = json;


class @PhoneNumber
    constructor: (json) ->
        if (json) then $.extend(this, json);