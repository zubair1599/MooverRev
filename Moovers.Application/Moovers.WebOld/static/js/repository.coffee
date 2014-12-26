this.Repository =
    employees: []
    vehicles: []
    getEmployee: (displayname) ->
        byName = _.find(this.employees, (emp) -> emp.getDisplayValue() == displayname)
        if(byName is Undefined) 
             return _.find(this.employees, (emp) -> emp.EmployeeID == displayname)
        return   byName

    getVehicle: (displayname) ->
        byName = _.find(this.vehicles, (v) -> v.getDisplayValue() == displayname)
        return byName || _.find(this.vehicles, (v) -> v.VehicleID == displayname)

    init: (employees, vehicles) ->
        this.employees = _.map(employees, (e) -> new Employee(e))
        this.vehicles = _.map(vehicles, (v) -> new Vehicle(v))