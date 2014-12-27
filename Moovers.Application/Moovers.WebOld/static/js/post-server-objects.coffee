# <reference path="references.js" />
###
    global PostingTemplates,Utility,SearchFunctions
    post-server-objects.js
###
class Employee
    constructor: (data) ->
        $.extend(this, data);

    getDisplayValue: () -> "#{@NameFirst} #{@NameLast} (#{@Lookup})";
    displayName: () -> "#{@NameFirst}  #{@NameLast}";

window.Employee = Employee;

class Vehicle
    constructor: (data) ->
        $.extend(this, data);

    getDisplayValue: -> "#{@Name} (#{@Lookup})";

window.Vehicle = Vehicle;

class Vehicle_Rel
    constructor: (data, posting) ->
        this.Vehicle = if (data instanceof Vehicle) then data else new Vehicle(data.Vehicle);
        this.Posting = posting;
        this.VehicleID = data.VehicleID;
        this.IsRemoved = data.IsRemoved || false;

class Employee_Rel
    constructor: (data, posting) ->
        this.Employee = if (data instanceof Employee) then data else new Employee(data.Employee);
        this.Posting = posting;
        this.EmployeeID = data.EmployeeID;
        this.Wage = data.Wage ? 0;
        this.Tip =  data.Tip ? 0;
        this.PerDiem =  data.PerDiem ? 0;
        this.Bonus = data.Bonus ? 0;
        this.Commission = data.Commission ? 0;
        this.Hours = data.Hours ? 0;
        this.IsRemoved = data.IsRemoved || false;
        this.ForceNoDriver = data.ForceNoDriver || false;

        isCrewDriver = _.chain((this.Posting || {}).Crew_Drivers).map( (i) -> i.EmployeeID ).contains(data.EmployeeID).value();

        this.IsDriver = data.IsDriver || (isCrewDriver && !this.ForceNoDriver)  || false;

    getCommissionPercent: ->
        price = this.Posting.calculateMovingCost();
        commission = this.Commission;
        if (price == 0 || commission == 0)
            return 0;

        return Utility.roundCurrency((commission / price) * 100);

    getTotal: () -> Utility.roundCurrency(this.Commission + this.Bonus + this.Tip);
    
    getManHourRate: () ->
        if (this.Posting.IsHourly)
            return 0;

        guaranteedPrice = this.Posting.GuaranteedPricing.GuaranteedPrice;
        manHours = this.Posting.calculateManHours();
        if (!manHours)
            return 0;

        return guaranteedPrice / manHours;

    getCommission: () ->
        price = this.Posting.calculateMovingCost();
        hours = this.Posting.calculateManHours();
        commissionpercent = if (price > 500) then .25 else .20;
        if (hours == 0)
            return 0
        else
            ( (price * commissionpercent) / hours) * this.Hours;


class Posting
    constructor: (data, container) ->
        posting = this;
        $.extend(posting, data);
        this.Date = new Date(data.Date);
        this.autoCalc = true;
        this.hasChanged = false;
        this.delayedHasChanged = false;
        this.container = container;
        this.IsCurrent = !_.isUndefined(container);
        this.StorageAccount = JSON.parse(data.StorageAccount);
        this.Crew_Drivers = _(data.Crew_Employee_Rels).filter( (i) -> i.IsDriver ).map( (i) -> new Employee(i.Employee));
        this.Employee_Rels = _.map(data.Employee_Rels, (i) -> new Employee_Rel(i, posting) );
        this.Vehicle_Rels = _.map(data.Vehicle_Rels, (v) -> new Vehicle_Rel(v, posting) );
        this.Crew_Employees = _.map(data.Crew_Employee_Rels, (e) -> new Employee(e.Employee) );
        this.Crew_Vehicles = _.map(data.Crew_Vehicle_Rels, (v) -> new Vehicle(v)  );

        employeeIds = _.pluck(posting.Employee_Rels, "EmployeeID");
        _.each(posting.Crew_Employees, (e) ->
            if (!_.contains(employeeIds, e.EmployeeID))
                posting.addEmployee(e, false);
        );

        vehicleIds = _.pluck(posting.Vehicle_Rels, "VehicleID");
        _.each(posting.Crew_Vehicles, (e) ->
            if (!_.contains(vehicleIds, e.VehicleID))
                posting.addVehicle(e, false);
        );

        _.each(posting.OtherServices, (s) -> s.Posting = posting );

        this.Siblings = _.map(data.Siblings, (s) -> new Posting(s));

    retemplate: () ->
        scroll = window.scrollY;
        tmpl = PostingTemplates.templates.posting;
        markup = tmpl(this);
        
        this.container.find("[autocomplete]").each( () ->
            if ($(this).data("autocomplete"))
                $(this).autocomplete("destroy");
        );

        ret = this.container.empty().append(markup);
        container = this.container;
        setTimeout( () ->
            ret.find("input").trigger("change");
            PostingTemplates.onTemplate(container);
        , 10);

        this.updateCalculatedFields();
        window.scroll(window.scrollX, scroll);
        return ret;

    retemplateEmployees: () ->
        container = this.container.find(".employee-job-post tbody");
        markup = this.RenderEmployees();
        container.html(markup);

    setEmployeeField: (employeeid, propname, val) ->
        if (this.IsComplete)
            return;

        this.hasChanged = true;
        employee = _.find(this.Employee_Rels, (i) -> i.EmployeeID == employeeid );

        employee[propname] = val;
        this.updateCalculatedFields();

    setCustomField: (id, val) ->
        if (this.IsComplete)
            return;

        this.hasChanged = true;
        cust = _.find(this.OtherServices, (i) -> i.ServiceID == id );

        if (cust)
            cust.Price = val;

        this.updateCalculatedFields();

    setField: (propname, val) ->
        if (this.IsComplete)
            return;

        isLiteral = (o) -> 
            return Object.prototype.toString.call(o) == "[object Object]";

        this.hasChanged = true;
        currentObj = this;

        props = propname.split(/\./g)
        for prop in props.slice(0, props.length - 1)
            currentObj = currentObj[prop];
            if (!isLiteral(currentObj))
                throw "Set Prop: Error : Cannot set " + prop;
        
        currentObj[props[props.length - 1]] = val;
        this.updateCalculatedFields();

    getTdClass: () ->
        if (this.isEditable()) then "input-td" 
        else if (this.IsRemoved) then "disabled"
        else ""
    
    isEditable: () -> !this.IsComplete && this.IsCurrent;
    
    updateCalculatedFields: () ->
        hourDisplay = this.container.find("span.hour-display");
        movingPriceDisplay = this.container.find("span.moving-services-price");
        totalDisplay = this.container.find("span.total-display");
        balanceDisplay = this.container.find("span.balance-display");
        guaranteedDisplay = this.container.find("span[name=totalMovePrice]");

        hourDisplay.text(Utility.formatCurrency(this.calculateMovingHours(), "", ""));
        totalDisplay.text(Utility.formatCurrency(this.calculateTotalCost()));
        post = this;
        _.each(this.Employee_Rels, (rel) ->
            row = post.container.find("tr[data-employeeid=" + rel.EmployeeID + "]");
            row.find("[name=Commission_Percent]").val(rel.getCommissionPercent());
            row.find(".Commission_Percent").text(rel.getCommissionPercent());
            row.find(".total-pay").text(Utility.formatCurrency(rel.getTotal()));
            row.find(".man-hour-rate").text(Utility.formatCurrency(rel.getManHourRate(), ""));
        );

        if (this.IsHourly)
            movingPriceDisplay.text(Utility.formatCurrency(this.calculateMovingCost()));
            balance = this.calculateBalance();
            balanceDisplay.text(Utility.formatCurrency(balance));
            balanceDisplay.toggleClass("red", balance != 0);
        else
            guaranteedPrice = this.GuaranteedPricing.GuaranteedPrice;
            guaranteedDisplay.text(Utility.formatCurrency(this.calculateTotalCost()));
            movingPriceDisplay.text(Utility.formatCurrency(guaranteedPrice));
            balanceDisplay.text(Utility.formatCurrency(0));
    
    RenderMovingServices: () ->
        tmpl = PostingTemplates.templates.moving_service_rel;
        post = this;
        posts = _.sortBy(_.flatten([post.Siblings, post]), (p) ->
            p.IsCurrent = (p == post);
            return "" + p.Date + "\u0000" + !p.IsCancelled;
        );

        return _.map(posts, (sib) -> return tmpl(sib) ).join("");
    
    RenderCustomServices: () ->
        services = this.OtherServices;
        tmpl = PostingTemplates.templates.posting_service;
        idx = 0;
        return _.map(services, (s) ->
            s.idx = idx;
            idx++;
            return tmpl(s);
        ).join("");
    
    RenderEmployees: (hide) ->
        tmpl = PostingTemplates.templates.employee_rel;
        idx = 0;
        return _.chain(this.Employee_Rels).sortBy( (i)  -> parseInt(i.Employee.Lookup, 10) || 0).map( (rel) ->
            rel.idx = idx;
            rel.hide = hide || false;
            idx = idx + 10;
            return tmpl(rel, { variable: "rel" });
        ).value().join("");
    
    RenderVehicles: () ->
        tmpl = PostingTemplates.templates.vehicle_rel;
        return _.map(this.Vehicle_Rels, (v) -> return tmpl(v) ).join("");
    
    calculateTotalCost: () ->
        totalCost = this.PackingMaterialsCost + this.PackingServiceCost + this.StorageFeesCost + this.ValuationCost;
        totalCost += _.sum(this.OtherServices, (s) -> return s.Price );
        totalCost += this.calculateMovingCost();
        return Utility.roundCurrency(totalCost);

    calculateTip: () -> _.sum(this.Employee_Rels, "Tip");
    
    calculateCostWithTip: () -> this.calculateTotalCost() + this.calculateTip();
    
    calculateBalance: () ->
        finalPrice = this.FinalPostedPrice;
        cost = this.calculateTotalCost();
        return Utility.roundCurrency(finalPrice - cost);
    
    calculateManHours: () ->
        rels = _(this.Employee_Rels).filter( (i) -> !i.IsRemoved );
        sibling_rels = _.chain(this.Siblings).pluck("Employee_Rels").flatten().filter( (i) -> !i.IsRemoved ).value()
        hours = _.chain(rels.concat(sibling_rels)).pluck("Hours").sum().value()
        return hours
    
    calculateMovingHours: () -> Utility.roundCurrency(_.sum(_.flatten([ this, this.Siblings ]), (item) -> item.PostingHours ));
    
    getHourlyPricingDescription: () ->
        if (!this.IsHourly)
            return "";

        posts = _.chain([this, this.Siblings ]).flatten();
        destinationFees = posts.sum( (item) -> item.HourlyPricing.FirstHourPrice - item.HourlyRate ).value();

        hoursList = {};
        addHours = (hours, rate) ->
            hoursList[rate] = hoursList[rate] || 0;
            hoursList[rate] += hours;

        posts.each( (item) -> 
            addHours(item.PostingHours, item.HourlyRate)
        );

        ret = Utility.formatCurrency(destinationFees, "$", "", 0) + " travel fee";
        _.each(hoursList, (hours, rate) ->
            ret += "\n " + hours + " hours @" + Utility.formatCurrency(rate) + " = " + Utility.formatCurrency(hours * rate);
        );

        return ret;
    
    calculateMovingCost: () ->
        if (this.IsHourly)
            group = _.flatten([this, this.Siblings]);
            return _.sum(group, (item) ->
                if (item.IsCancelled)
                    return 0;

                firstHour = item.HourlyPricing.FirstHourPrice;
                hourlyRate = item.HourlyRate;
                hours = item.PostingHours;
                return Utility.roundCurrency(firstHour + (Math.max(hours, 1) - 1) * hourlyRate);
            );
        else
            return this.GuaranteedPricing.GuaranteedPrice;

    addCustomService: (name) ->
        if (this.IsComplete)
            return;

        item = {
            ServiceID : Utility.randomid(),
            PostingID: this.PostingID,
            Posting: this,
            Description: name,
            Type: 4,
            Price: 0
        };

        this.OtherServices.push(item);
        this.retemplate();
    
    removeService: (serviceid) ->
        return if (this.IsComplete)
        this.OtherServices = _.reject(this.OtherServices, (i) -> i.ServiceID == serviceid )
        this.retemplate()
        this.updateCalculatedFields()
    
    addEmployee: (employee, retemplate) ->
        return if (this.IsComplete)

        retemplate = retemplate ? true

        rel = _(this.Employee_Rels).filter(SearchFunctions.employee(employee))

        if (_.any(rel))
            rel[0].IsRemoved = false
        else
            posting = this
            posting.removeEmployee(employee)
            rel = new Employee_Rel(employee, posting)
            rel.Wage = employee.Wage
            rel.Hours = posting.PostingHours + posting.DriveHours
            rel.IsRemoved = false
            rel.Bonus = 0
            rel.Commission = 0
            rel.Tip = 0
            rel.PerDiem = 0
            posting.Employee_Rels.push(rel)

        if (retemplate)
            this.retemplate();

    addVehicle: (vehicle, retemplate) ->
        return if this.IsComplete
        
        retemplate = retemplate ? true

        rel = _(this.Vehicle_Rels).filter(SearchFunctions.vehicle(vehicle));
        if (_.any(rel))
            rel[0].IsRemoved = false;
        else
            this.removeVehicle(vehicle);
            rel = new Vehicle_Rel(vehicle);
            rel.Posting = this;
            this.Vehicle_Rels.push(rel);

        if (retemplate)
            this.retemplate();

    removeEmployee: (emp) ->
        if (this.IsComplete || !emp)
            return;
        
        findEmployee = SearchFunctions.employee(emp);
        byCrew = _(this.Crew_Employees).filter(findEmployee);
        cur = _(this.Employee_Rels).filter(findEmployee);
        if (_.any(byCrew) && _.any(cur))
            cur[0].IsRemoved = true;
        else
            this.Employee_Rels = _.reject(this.Employee_Rels, findEmployee);
    
    removeVehicle: (vehicle) ->
        if (this.IsComplete || !vehicle)
            return;
        
        findVehicle = SearchFunctions.vehicle(vehicle);
        byCrew = _(this.Crew_Vehicles).filter(findVehicle);
        cur = _(this.Vehicle_Rels).filter(findVehicle);

        if (_.any(byCrew) && _.any(cur))
            cur[0].IsRemoved = true;
        else
            this.Vehicle_Rels = _.reject(this.Vehicle_Rels, findVehicle);
    
    toServerObject: () ->
        return {
            id: this.PostingID,
            finalPostedPrice: this.FinalPostedPrice
            postingHours: this.PostingHours || 0
            driveHours: this.DriveHours || 0
            hourlyRate: this.HourlyRate
            firstHourRate: if (this.IsHourly) then this.HourlyPricing.FirstHourPrice else null
            packingMaterialsCost: this.PackingMaterialsCost
            packingServiceCost: this.PackingServiceCost
            storageFeesCost: this.StorageFeesCost
            replacementValuation: this.ValuationCost

            otherServices: JSON.stringify(_.map(this.OtherServices, (serv) ->
                return {
                    Description: serv.Description
                    QuoteID: serv.QuoteID
                    ServiceID: serv.ServiceID
                    Type: serv.Type
                    Price: serv.Price
                };
            ))

            employees: JSON.stringify(_.map(this.Employee_Rels, (rel) ->
                return {
                    EmployeeID: rel.EmployeeID
                    Bonus: rel.Bonus
                    Commission: rel.Commission
                    Hours: rel.Hours
                    IsRemoved: rel.IsRemoved
                    Tip: rel.Tip
                    PerDiem: rel.PerDiem
                    Wage: rel.Wage
                    IsDriver: rel.IsDriver
                };
            ))
            
            vehicles: JSON.stringify(_.map(this.Vehicle_Rels, (rel) ->
                return {
                    VehicleID: rel.VehicleID
                    IsRemoved: rel.IsRemoved
                };
            ))
        };

    cancel: () -> 
        this.IsCancelled = this.hasChanged = true;

    sync: (cb) ->
        if (this.IsComplete)
            this.hasChanged = false;
            return;

        if (!this.hasChanged)
            cb();
        else 
            url = SERVER.baseUrl + "PostingPage/SaveHourlyPostData/" + this.PostingID;
            if (!this.IsHourly)
                url = SERVER.baseUrl + "PostingPage/SaveGuaranteedPostData/" + this.PostingID;

            post = this;
            post.hasChanged = false;

            $.postq("posting", url, this.toServerObject(), (resp) ->
                if ($.isFunction(cb)) 
                    cb(resp)
                
                _.each(resp, (val, key) ->
                    item = _.find(post.OtherServices, (serv) -> return serv.ServiceID == key)
                    if (item) then item.ServiceID = val
                    post.container.find("input[name=serviceid][value=" + key + "]").val(val)
                    post.container.find("input[data-field=" + key + "]").data("field", val)
                )
            )

window.Posting = Posting;