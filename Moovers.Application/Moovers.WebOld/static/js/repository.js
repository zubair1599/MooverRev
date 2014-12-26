this.Repository = {
    employees: [],
    vehicles: [],
    getEmployee: function (displayname) {
        var byName;
        byName = _.find(this.employees, function (emp) {
            return emp.getDisplayValue() === displayname;
        });
        if (byName === window.Undefined) {
            return _.find(this.employees, function (emp) {
                return emp.EmployeeID === displayname;
            });
        }
        return byName;
    },
    getVehicle: function (displayname) {
        var byName;
        byName = _.find(this.vehicles, function (v) {
            return v.getDisplayValue() === displayname;
        });
        return byName || _.find(this.vehicles, function (v) {
            return v.VehicleID === displayname;
        });
    },
    init: function (employees, vehicles) {
        this.employees = _.map(employees, function (e) {
            return new Employee(e);
        });
        return this.vehicles = _.map(vehicles, function (v) {
            return new Vehicle(v);
        });
    }
};