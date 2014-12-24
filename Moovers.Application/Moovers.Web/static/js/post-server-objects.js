
/*
    global PostingTemplates,Utility,SearchFunctions
    post-server-objects.js
*/


(function() {
    var Employee, Employee_Rel, Posting, Vehicle, Vehicle_Rel;

    Employee = (function () {
        function Employee(data) {
            $.extend(this, data);
        }

        Employee.prototype.getDisplayValue = function () {
            return "" + this.NameFirst + " " + this.NameLast + " (" + this.Lookup + ")";
        };

        Employee.prototype.displayName = function () {
            return "" + this.NameFirst + "  " + this.NameLast;
        };

        return Employee;

    })();

    window.Employee = Employee;

    Vehicle = (function () {
        function Vehicle(data) {
            $.extend(this, data);
        }

        Vehicle.prototype.getDisplayValue = function () {
            return "" + this.Name + " (" + this.Lookup + ")";
        };

        return Vehicle;

    })();

    window.Vehicle = Vehicle;

    Vehicle_Rel = (function () {
        function Vehicle_Rel(data, posting) {
            this.Vehicle = data instanceof Vehicle ? data : new Vehicle(data.Vehicle);
            this.Posting = posting;
            this.VehicleID = data.VehicleID;
            this.IsRemoved = data.IsRemoved || false;
        }

        return Vehicle_Rel;

    })();

    Employee_Rel = (function () {
        function Employee_Rel(data, posting) {
            var isCrewDriver, _ref, _ref1, _ref2, _ref3, _ref4, _ref5;
            this.Employee = data instanceof Employee ? data : new Employee(data.Employee);
            this.Posting = posting;
            this.EmployeeID = data.EmployeeID;
            this.Wage = (_ref = data.Wage) != null ? _ref : 0;
            this.Tip = (_ref1 = data.Tip) != null ? _ref1 : 0;
            this.PerDiem = (_ref2 = data.PerDiem) != null ? _ref2 : 0;
            this.Bonus = (_ref3 = data.Bonus) != null ? _ref3 : 0;
            this.Commission = (_ref4 = data.Commission) != null ? _ref4 : 0;
            this.Hours = (_ref5 = data.Hours) != null ? _ref5 : 0;
            this.IsRemoved = data.IsRemoved || false;
            this.ForceNoDriver = data.ForceNoDriver || false;
            isCrewDriver = _.chain((this.Posting || {}).Crew_Drivers).map(function (i) {
                return i.EmployeeID;
            }).contains(data.EmployeeID).value();
            this.IsDriver = data.IsDriver || (isCrewDriver && !this.ForceNoDriver) || false;
        }

        Employee_Rel.prototype.getCommissionPercent = function () {
            var commission, price;
            price = this.Posting.calculateMovingCost();
            commission = this.Commission;
            if (price === 0 || commission === 0) {
                return 0;
            }
            return Utility.roundCurrency((commission / price) * 100);
        };

        Employee_Rel.prototype.getTotal = function () {
            return Utility.roundCurrency(this.Commission + this.Bonus + this.Tip);
        };

        Employee_Rel.prototype.getManHourRate = function () {
            var guaranteedPrice, manHours;
            if (this.Posting.IsHourly) {
                return 0;
            }
            guaranteedPrice = this.Posting.GuaranteedPricing.GuaranteedPrice;
            manHours = this.Posting.calculateManHours();
            if (!manHours) {
                return 0;
            }
            return guaranteedPrice / manHours;
        };

        Employee_Rel.prototype.getCommission = function () {
            var commissionpercent, hours, price;
            price = this.Posting.calculateMovingCost();
            hours = this.Posting.calculateManHours();
            commissionpercent = price > 500 ? .25 : .20;
            if (hours === 0) {
                return 0;
            } else {
                return ((price * commissionpercent) / hours) * this.Hours;
            }
        };

        return Employee_Rel;

    })();

    Posting = (function () {
        function Posting(data, container) {
            var employeeIds, posting, vehicleIds;
            posting = this;
            $.extend(posting, data);
            this.Date = new Date(data.Date);
            this.autoCalc = true;
            this.hasChanged = false;
            this.delayedHasChanged = false;
            this.container = container;
            this.IsCurrent = !_.isUndefined(container);
            this.StorageAccount = JSON.parse(data.StorageAccount);
            this.Crew_Drivers = _(data.Crew_Employee_Rels).filter(function (i) {
                return i.IsDriver;
            }).map(function (i) {
                return new Employee(i.Employee);
            });
            this.Employee_Rels = _.map(data.Employee_Rels, function (i) {
                return new Employee_Rel(i, posting);
            });
            this.Vehicle_Rels = _.map(data.Vehicle_Rels, function (v) {
                return new Vehicle_Rel(v, posting);
            });
            this.Crew_Employees = _.map(data.Crew_Employee_Rels, function (e) {
                return new Employee(e.Employee);
            });
            this.Crew_Vehicles = _.map(data.Crew_Vehicle_Rels, function (v) {
                return new Vehicle(v);
            });
            employeeIds = _.pluck(posting.Employee_Rels, "EmployeeID");
            _.each(posting.Crew_Employees, function (e) {
                if (!_.contains(employeeIds, e.EmployeeID)) {
                    return posting.addEmployee(e, false);
                }
            });
            vehicleIds = _.pluck(posting.Vehicle_Rels, "VehicleID");
            _.each(posting.Crew_Vehicles, function (e) {
                if (!_.contains(vehicleIds, e.VehicleID)) {
                    return posting.addVehicle(e, false);
                }
            });
            _.each(posting.OtherServices, function (s) {
                return s.Posting = posting;
            });
            this.Siblings = _.map(data.Siblings, function (s) {
                return new Posting(s);
            });
        }

        Posting.prototype.retemplate = function () {
            var container, markup, ret, scroll, tmpl;
            scroll = window.scrollY;
            tmpl = PostingTemplates.templates.posting;
            markup = tmpl(this);
            this.container.find("[autocomplete]").each(function () {
                if ($(this).data("autocomplete")) {
                    return $(this).autocomplete("destroy");
                }
            });
            ret = this.container.empty().append(markup);
            container = this.container;
            setTimeout(function () {
                ret.find("input").trigger("change");
                return PostingTemplates.onTemplate(container);
            }, 10);
            this.updateCalculatedFields();
            window.scroll(window.scrollX, scroll);
            return ret;
        };

        Posting.prototype.retemplateEmployees = function () {
            var container, markup;
            container = this.container.find(".employee-job-post tbody");
            markup = this.RenderEmployees();
            return container.html(markup);
        };

        Posting.prototype.setEmployeeField = function (employeeid, propname, val) {
            var employee;
            if (this.IsComplete) {
                return;
            }
            this.hasChanged = true;
            employee = _.find(this.Employee_Rels, function (i) {
                return i.EmployeeID === employeeid;
            });
            employee[propname] = val;
            return this.updateCalculatedFields();
        };

        Posting.prototype.setCustomField = function (id, val) {
            var cust;
            if (this.IsComplete) {
                return;
            }
            this.hasChanged = true;
            cust = _.find(this.OtherServices, function (i) {
                return i.ServiceID === id;
            });
            if (cust) {
                cust.Price = val;
            }
            return this.updateCalculatedFields();
        };

        Posting.prototype.setField = function (propname, val) {
            var currentObj, isLiteral, prop, props, _i, _len, _ref;
            if (this.IsComplete) {
                return;
            }
            isLiteral = function (o) {
                return Object.prototype.toString.call(o) === "[object Object]";
            };
            this.hasChanged = true;
            currentObj = this;
            props = propname.split(/\./g);
            _ref = props.slice(0, props.length - 1);
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                prop = _ref[_i];
                currentObj = currentObj[prop];
                if (!isLiteral(currentObj)) {
                    throw "Set Prop: Error : Cannot set " + prop;
                }
            }
            currentObj[props[props.length - 1]] = val;
            return this.updateCalculatedFields();
        };

        Posting.prototype.getTdClass = function () {
            if (this.isEditable()) {
                return "input-td";
            } else if (this.IsRemoved) {
                return "disabled";
            } else {
                return "";
            }
        };

        Posting.prototype.isEditable = function () {
            return !this.IsComplete && this.IsCurrent;
        };

        Posting.prototype.updateCalculatedFields = function () {
            var balance, balanceDisplay, guaranteedDisplay, guaranteedPrice, hourDisplay, movingPriceDisplay, post, totalDisplay;
            hourDisplay = this.container.find("span.hour-display");
            movingPriceDisplay = this.container.find("span.moving-services-price");
            totalDisplay = this.container.find("span.total-display");
            balanceDisplay = this.container.find("span.balance-display");
            guaranteedDisplay = this.container.find("span[name=totalMovePrice]");
            hourDisplay.text(Utility.formatCurrency(this.calculateMovingHours(), "", ""));
            totalDisplay.text(Utility.formatCurrency(this.calculateTotalCost()));
            post = this;
            _.each(this.Employee_Rels, function (rel) {
                var row;
                row = post.container.find("tr[data-employeeid=" + rel.EmployeeID + "]");
                row.find("[name=Commission_Percent]").val(rel.getCommissionPercent());
                row.find(".Commission_Percent").text(rel.getCommissionPercent());
                row.find(".total-pay").text(Utility.formatCurrency(rel.getTotal()));
                return row.find(".man-hour-rate").text(Utility.formatCurrency(rel.getManHourRate(), ""));
            });
            if (this.IsHourly) {
                movingPriceDisplay.text(Utility.formatCurrency(this.calculateMovingCost()));
                balance = this.calculateBalance();
                balanceDisplay.text(Utility.formatCurrency(balance));
                return balanceDisplay.toggleClass("red", balance !== 0);
            } else {
                guaranteedPrice = this.GuaranteedPricing.GuaranteedPrice;
                guaranteedDisplay.text(Utility.formatCurrency(this.calculateTotalCost()));
                movingPriceDisplay.text(Utility.formatCurrency(guaranteedPrice));
                return balanceDisplay.text(Utility.formatCurrency(0));
            }
        };

        Posting.prototype.RenderMovingServices = function () {
            var post, posts, tmpl;
            tmpl = PostingTemplates.templates.moving_service_rel;
            post = this;
            posts = _.sortBy(_.flatten([post.Siblings, post]), function (p) {
                p.IsCurrent = p === post;
                return "" + p.Date + "\u0000" + !p.IsCancelled;
            });
            return _.map(posts, function (sib) {
                return tmpl(sib);
            }).join("");
        };

        Posting.prototype.RenderCustomServices = function () {
            var idx, services, tmpl;
            services = this.OtherServices;
            tmpl = PostingTemplates.templates.posting_service;
            idx = 0;
            return _.map(services, function (s) {
                s.idx = idx;
                idx++;
                return tmpl(s);
            }).join("");
        };

        Posting.prototype.RenderEmployees = function (hide) {
            var idx, tmpl;
            tmpl = PostingTemplates.templates.employee_rel;
            idx = 0;
            return _.chain(this.Employee_Rels).sortBy(function (i) {
                return parseInt(i.Employee.Lookup, 10) || 0;
            }).map(function (rel) {
                rel.idx = idx;
                rel.hide = hide || false;
                idx = idx + 10;
                return tmpl(rel, {
                    variable: "rel"
                });
            }).value().join("");
        };

        Posting.prototype.RenderVehicles = function () {
            var tmpl;
            tmpl = PostingTemplates.templates.vehicle_rel;
            return _.map(this.Vehicle_Rels, function (v) {
                return tmpl(v);
            }).join("");
        };

        Posting.prototype.calculateTotalCost = function () {
            var totalCost;
            totalCost = this.PackingMaterialsCost + this.PackingServiceCost + this.StorageFeesCost + this.ValuationCost;
            totalCost += _.sum(this.OtherServices, function (s) {
                return s.Price;
            });
            totalCost += this.calculateMovingCost();
            return Utility.roundCurrency(totalCost);
        };

        Posting.prototype.calculateTip = function () {
            return _.sum(this.Employee_Rels, "Tip");
        };

        Posting.prototype.calculateCostWithTip = function () {
            return this.calculateTotalCost() + this.calculateTip();
        };

        Posting.prototype.calculateBalance = function () {
            var cost, finalPrice;
            finalPrice = this.FinalPostedPrice;
            cost = this.calculateTotalCost();
            return Utility.roundCurrency(finalPrice - cost);
        };

        Posting.prototype.calculateManHours = function () {
            var hours, rels, sibling_rels;
            rels = _(this.Employee_Rels).filter(function (i) {
                return !i.IsRemoved;
            });
            sibling_rels = _.chain(this.Siblings).pluck("Employee_Rels").flatten().filter(function (i) {
                return !i.IsRemoved;
            }).value();
            hours = _.chain(rels.concat(sibling_rels)).pluck("Hours").sum().value();
            return hours;
        };

        Posting.prototype.calculateMovingHours = function () {
            return Utility.roundCurrency(_.sum(_.flatten([this, this.Siblings]), function (item) {
                return item.PostingHours;
            }));
        };

        Posting.prototype.getHourlyPricingDescription = function () {
            var addHours, destinationFees, hoursList, posts, ret;
            if (!this.IsHourly) {
                return "";
            }
            posts = _.chain([this, this.Siblings]).flatten();
            destinationFees = posts.sum(function (item) {
                return item.HourlyPricing.FirstHourPrice - item.HourlyRate;
            }).value();
            hoursList = {};
            addHours = function (hours, rate) {
                hoursList[rate] = hoursList[rate] || 0;
                return hoursList[rate] += hours;
            };
            posts.each(function (item) {
                return addHours(item.PostingHours, item.HourlyRate);
            });
            ret = Utility.formatCurrency(destinationFees, "$", "", 0) + " travel fee";
            _.each(hoursList, function (hours, rate) {
                return ret += "\n " + hours + " hours @" + Utility.formatCurrency(rate) + " = " + Utility.formatCurrency(hours * rate);
            });
            return ret;
        };

        Posting.prototype.calculateMovingCost = function () {
            var group;
            if (this.IsHourly) {
                group = _.flatten([this, this.Siblings]);
                return _.sum(group, function (item) {
                    var firstHour, hourlyRate, hours;
                    if (item.IsCancelled) {
                        return 0;
                    }
                    firstHour = item.HourlyPricing.FirstHourPrice;
                    hourlyRate = item.HourlyRate;
                    hours = item.PostingHours;
                    return Utility.roundCurrency(firstHour + (Math.max(hours, 1) - 1) * hourlyRate);
                });
            } else {
                return this.GuaranteedPricing.GuaranteedPrice;
            }
        };

        Posting.prototype.addCustomService = function (name) {
            var item;
            if (this.IsComplete) {
                return;
            }
            item = {
                ServiceID: Utility.randomid(),
                PostingID: this.PostingID,
                Posting: this,
                Description: name,
                Type: 4,
                Price: 0
            };
            this.OtherServices.push(item);
            return this.retemplate();
        };

        Posting.prototype.removeService = function (serviceid) {
            if (this.IsComplete) {
                return;
            }
            this.OtherServices = _.reject(this.OtherServices, function (i) {
                return i.ServiceID === serviceid;
            });
            this.retemplate();
            return this.updateCalculatedFields();
        };

        Posting.prototype.addEmployee = function (employee, retemplate) {
            var posting, rel;
            if (this.IsComplete) {
                return;
            }
            retemplate = retemplate != null ? retemplate : true;
            rel = _(this.Employee_Rels).filter(SearchFunctions.employee(employee));
            if (_.any(rel)) {
                rel[0].IsRemoved = false;
            } else {
                posting = this;
                posting.removeEmployee(employee);
                rel = new Employee_Rel(employee, posting);
                rel.Wage = employee.Wage;
                rel.Hours = posting.PostingHours + posting.DriveHours;
                rel.IsRemoved = false;
                rel.Bonus = 0;
                rel.Commission = 0;
                rel.Tip = 0;
                rel.PerDiem = 0;
                posting.Employee_Rels.push(rel);
            }
            if (retemplate) {
                return this.retemplate();
            }
        };

        Posting.prototype.addVehicle = function (vehicle, retemplate) {
            var rel;
            if (this.IsComplete) {
                return;
            }
            retemplate = retemplate != null ? retemplate : true;
            rel = _(this.Vehicle_Rels).filter(SearchFunctions.vehicle(vehicle));
            if (_.any(rel)) {
                rel[0].IsRemoved = false;
            } else {
                this.removeVehicle(vehicle);
                rel = new Vehicle_Rel(vehicle);
                rel.Posting = this;
                this.Vehicle_Rels.push(rel);
            }
            if (retemplate) {
                return this.retemplate();
            }
        };

        Posting.prototype.removeEmployee = function (emp) {
            var byCrew, cur, findEmployee;
            if (this.IsComplete || !emp) {
                return;
            }
            findEmployee = SearchFunctions.employee(emp);
            byCrew = _(this.Crew_Employees).filter(findEmployee);
            cur = _(this.Employee_Rels).filter(findEmployee);
            if (_.any(byCrew) && _.any(cur)) {
                return cur[0].IsRemoved = true;
            } else {
                return this.Employee_Rels = _.reject(this.Employee_Rels, findEmployee);
            }
        };

        Posting.prototype.removeVehicle = function (vehicle) {
            var byCrew, cur, findVehicle;
            if (this.IsComplete || !vehicle) {
                return;
            }
            findVehicle = SearchFunctions.vehicle(vehicle);
            byCrew = _(this.Crew_Vehicles).filter(findVehicle);
            cur = _(this.Vehicle_Rels).filter(findVehicle);
            if (_.any(byCrew) && _.any(cur)) {
                return cur[0].IsRemoved = true;
            } else {
                return this.Vehicle_Rels = _.reject(this.Vehicle_Rels, findVehicle);
            }
        };

        Posting.prototype.toServerObject = function () {
            return {
                id: this.PostingID,
                finalPostedPrice: this.FinalPostedPrice,
                postingHours: this.PostingHours || 0,
                driveHours: this.DriveHours || 0,
                hourlyRate: this.HourlyRate,
                firstHourRate: this.IsHourly ? this.HourlyPricing.FirstHourPrice : null,
                packingMaterialsCost: this.PackingMaterialsCost,
                packingServiceCost: this.PackingServiceCost,
                storageFeesCost: this.StorageFeesCost,
                replacementValuation: this.ValuationCost,
                otherServices: JSON.stringify(_.map(this.OtherServices, function (serv) {
                    return {
                        Description: serv.Description,
                        QuoteID: serv.QuoteID,
                        ServiceID: serv.ServiceID,
                        Type: serv.Type,
                        Price: serv.Price
                    };
                })),
                employees: JSON.stringify(_.map(this.Employee_Rels, function (rel) {
                    return {
                        EmployeeID: rel.EmployeeID,
                        Bonus: rel.Bonus,
                        Commission: rel.Commission,
                        Hours: rel.Hours,
                        IsRemoved: rel.IsRemoved,
                        Tip: rel.Tip,
                        PerDiem: rel.PerDiem,
                        Wage: rel.Wage,
                        IsDriver: rel.IsDriver
                    };
                })),
                vehicles: JSON.stringify(_.map(this.Vehicle_Rels, function (rel) {
                    return {
                        VehicleID: rel.VehicleID,
                        IsRemoved: rel.IsRemoved
                    };
                }))
            };
        };

        Posting.prototype.cancel = function () {
            return this.IsCancelled = this.hasChanged = true;
        };

        Posting.prototype.sync = function (cb) {
            var post, url;
            if (this.IsComplete) {
                this.hasChanged = false;
                return;
            }
            if (!this.hasChanged) {
                return cb();
            } else {
                url = SERVER.baseUrl + "PostingPage/SaveHourlyPostData/" + this.PostingID;
                if (!this.IsHourly) {
                    url = SERVER.baseUrl + "PostingPage/SaveGuaranteedPostData/" + this.PostingID;
                }
                post = this;
                post.hasChanged = false;
                return $.postq("posting", url, this.toServerObject(), function (resp) {
                    if ($.isFunction(cb)) {
                        cb(resp);
                    }
                    return _.each(resp, function (val, key) {
                        var item;
                        item = _.find(post.OtherServices, function (serv) {
                            return serv.ServiceID === key;
                        });
                        if (item) {
                            item.ServiceID = val;
                        }
                        post.container.find("input[name=serviceid][value=" + key + "]").val(val);
                        return post.container.find("input[data-field=" + key + "]").data("field", val);
                    });
                });
            }
        };

        return Posting;

    })();

    window.Posting = Posting;;

}).call(this);
