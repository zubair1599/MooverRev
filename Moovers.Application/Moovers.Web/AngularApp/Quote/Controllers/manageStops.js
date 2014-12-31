quoteApp.controller('manageStops', ['quoteFactory','addressFactory','$timeout','$scope','$element','$window',manageStops]);

function manageStops(quoteFactory, addressFactory, $timeout, $scope, $element, $window) {

   
    $scope.selectedStop = {};
    $scope.allstates = '';
    $scope.stopDialog = false;

    $scope.statesCodes = [];
    $scope.states = [];
    $scope.SearchAddress = '';
    $scope.current = 1;
    $scope.BuildingType = ['Residential', 'Commercial', 'Government'];
    $scope.Buildings = [];
    $scope.BuildingsCodes = [];
    $scope.loadFirstTime = true;
    
    
    

    $scope.ScopeDialogInitialize = function(id) {

      
        for (var i = 0; i < $scope.$parent.selectedQuote.Stops.length; i++) {
            if ($scope.$parent.selectedQuote.Stops[i].id == id) {
                $scope.selectedStop = $scope.$parent.selectedQuote.Stops[i];
            }
        }

        $scope.InitEditStop();


    };

    $scope.InitEditStop = function() {
        
        $scope.SearchAddress = '';
        $scope.stopDialog = true;
        addressFactory.GetStates().then(function (states) {
            $.each(JSON.parse(states), function (index, fb) {

                $scope.statesCodes = $scope.statesCodes.concat(fb);
                $scope.states = $scope.states.concat(index);
            });



            var uri = encodeURI($scope.selectedStop.address.replace(/[#?$+,\/:&;=@%{}|\\\[\]\^~]/g, ""));
            var url = "http://maps.googleapis.com/maps/api/streetview?size=400x300&location=" + uri + "&sensor=false";
            $element.find('#mapImg').attr("src", url);


            $timeout(function () {

                addressFactory.GetBuildingType($scope.BuildingType[0]).then(function (buildingsz) {
                    var tmp = JSON.parse(buildingsz.buildings);
                    $scope.Buildings[0] = [];
                    $scope.BuildingsCodes[0] = [];

                    $scope.Buildings[1] = [];
                    $scope.BuildingsCodes[1] = [];

                    $scope.Buildings[2] = [];
                    $scope.BuildingsCodes[2] = [];


                    $.each((tmp), function (index, fb) {

                        $scope.Buildings[0] = $scope.Buildings[0].concat(fb);
                        $scope.BuildingsCodes[0] = $scope.BuildingsCodes[0].concat(index);
                    });



                    $timeout(function () {
                        addressFactory.GetBuildingType($scope.BuildingType[1]).then(function (buildingso) {
                            tmp = JSON.parse(buildingso.buildings);
                            $.each((tmp), function (index, fb) {

                                $scope.Buildings[1] = $scope.Buildings[1].concat(fb);
                                $scope.BuildingsCodes[1] = $scope.BuildingsCodes[1].concat(index);
                            });

                            $timeout(function () {
                                addressFactory.GetBuildingType($scope.BuildingType[2]).then(function (buildingst) {

                                    tmp = JSON.parse(buildingst.buildings);
                                    $.each((tmp), function (index, fb) {

                                        $scope.Buildings[2] = $scope.Buildings[2].concat(fb);
                                        $scope.BuildingsCodes[2] = $scope.BuildingsCodes[2].concat(index);
                                    });
                                });
                            });

                        });
                    });


                });
            });
            //$scope.$apply();
        });


        $scope.SetPreviousSelectedStop();
        $scope.InitMaps();




        $timeout(function () {
            $scope.$apply();
        });

    };

    $scope.Clear = function() {

        $scope.selectedStop = {};
        $scope.stopDialog = false ;

    };

    $scope.AddStop = function () {

        $scope.$parent.selectedQuote.Stops.push($scope.selectedStop);
        quoteFactory.UpdateStops($scope.$parent.selectedQuote.QuoteID, $scope.$parent.selectedQuote.Stops);
        quoteFactory.servicePromise.promise.then(function (stopsIdjson) {
            $scope.$parent.RefreshStops();//Init();
        });
    };

    $scope.DeleteStop = function() {

        var id = $scope.selectedStop.id;
        quoteFactory.DeleteStop(id);
        quoteFactory.servicePromise.promise.then(function (data) {
            //alert(data);
            if (data=='OK') {
                $scope.$parent.RefreshStops();
               
            }
        });
        $scope.Clear();

    };


    $scope.InitMaps = function() {


        var options = {
            componentRestrictions: { country: "us" }
        };

        var input = $element.find('#searchTextField')[0];
        var autocomplete = new $window.google.maps.places.Autocomplete(input, options);
        $window.google.maps.event.addListener(autocomplete, "place_changed", function () {
            var place = autocomplete.getPlace();
            function getVal(name, type) {
                type = type || "long_name";
                var ret = _.filter(place.address_components, function (i) {
                    return _.any(i.types, function (r) {
                        return r === name;
                    });
                })[0];

                return (ret ? ret[type] : "");
            }

            if (place.address_components) {
                var vals = {
                    street1: getVal("street_number") + " " + getVal("route"),
                    city: getVal("locality"),
                    state: getVal("administrative_area_level_1", "short_name"),
                    zip: getVal("postal_code")
                };

                $scope.selectedStop.street1 = vals.street1;
                $scope.selectedStop.city = vals.city;
                $scope.selectedStop.state = vals.state;
                $scope.selectedStop.zip = vals.zip;

                //element.find('#street1').val(vals.street1);
                //element.find('#city').val(vals.city);
                //element.find('#state').val(vals.state);
                //element.find('#zip').val(vals.zip);
                var address = new Object();
                address.street1 = vals.street1;
                address.street2 = '';
                address.city = vals.city;
                address.state = vals.state;
                address.zip = vals.zip;
                address.display = vals.street1 + ' ' + vals.street2 + ' ' + vals.city + ' , ' + vals.state + ' ' + vals.zip;

                if (address.street1) {
                    var uri = encodeURI(address.display.replace(/[#?$+,\/:&;=@%{}|\\\[\]\^~]/g, ""));
                    var url = "http://maps.googleapis.com/maps/api/streetview?size=400x300&location=" + uri + "&sensor=false";
                    $element.find('#mapImg').attr("src", url);

                }



                quoteFactory.AddressSuggestions(address);
                quoteFactory.servicePromise.promise.then(function (verifiedAddress) {


                    $scope.loadFirstTime = false;



                    if (verifiedAddress.length > 0) {

                        $scope.verifiedAddress = verifiedAddress;
                        // alert();

                        $element.find('#unverifiedAddressResult').empty();
                        $element.find('#verifiedAddressResult').empty();
                        for (var i = 0; i < $scope.verifiedAddress.length; i++) {

                            var mainDiv = $('<div> </div>');
                            mainDiv.appendTo($element.find('#verifiedAddressResult'));

                            var id = "rdo" + i;

                            var radioBtn = $('<input style="float:left" type="radio" data-index="' + i + '" name="address" id="' + id + '" />');
                            var divs = $('<label for="' + id + '" >' + $scope.verifiedAddress[i].displayString() + '</label>');


                            radioBtn.click(function () {
                                var t = $(this).attr('data-index');
                                $scope.selectedStop.street1 = $scope.verifiedAddress[t].json.delivery_line_1;
                                $scope.selectedStop.zip = $scope.verifiedAddress[t].json.components.zipcode;
                                $scope.selectedStop.city = $scope.verifiedAddress[t].json.components.city_name;
                                $scope.selectedStop.state = $scope.verifiedAddress[t].json.components.state_abbreviation;
                                $scope.$apply();
                                //alert($scope.verifiedAddress[t].json.delivery_line_1);
                            });

                            radioBtn.appendTo($element.find(mainDiv));
                            divs.appendTo($element.find(mainDiv));
                            if (i == $scope.verifiedAddress.length - 1) {
                                break;
                            }
                        }
                    } else {



                        $element.find('#unverifiedAddressResult').empty();
                        $element.find('#verifiedAddressResult').empty();
                        var mainDiv = $('<div style="width:100%"></div>');
                        mainDiv.appendTo($element.find('#unverifiedAddressResult'));

                        var id = "rdo" + i;

                        var radioBtn = $('<input style="float:left" type="radio" name="address" id="' + id + '" />');
                        var divs = $('<label for="' + id + '" >' + vals.street1 + " , " + vals.city + " , " + vals.state + " , " + vals.zip + '</label>');

                        radioBtn.appendTo($element.find(mainDiv));
                        divs.appendTo($element.find(mainDiv));

                    }

                    $scope.SetPreviousSelectedStop();



                });





                //Utility.objectToForm(vals, Stops.elements.stopModal.find("form.addr-search"));
                if (vals.zip) {
                    // Stops.elements.stopModal.find("form.addr-search").find("[type=submit]").click();
                }
            } else if (place.name) {
                $element.find('#street1').val(place.name).focus();
                //Stops.elements.stopModal.find("form.addr-search [name=street1]").val(place.name).focus();

            }
        });


    };


    $scope.FindAddress = function () {

        $scope.loadFirstTime = false;

        var address = new Object();
        address.street1 = $scope.selectedStop.street1;
        address.street2 = $scope.selectedStop.street2;
        address.city = $scope.selectedStop.city;
        address.state = $scope.selectedStop.state;
        address.zip = $scope.selectedStop.zip;
        address.display = address.street1 + ' ' + address.street2 + ' ' + address.city + ' , ' + address.state + ' ' + address.zip;

        if (address.street1) {
            var uri = encodeURI(address.display.replace(/[#?$+,\/:&;=@%{}|\\\[\]\^~]/g, ""));
            var url = "http://maps.googleapis.com/maps/api/streetview?size=400x300&location=" + uri + "&sensor=false";
           $element.find('#mapImg').attr("src", url);

        }

        quoteFactory.AddressSuggestions(address);
        quoteFactory.servicePromise.promise.then(function (verifiedAddress) {


            $element.find('#unverifiedAddressResult').empty();
            $element.find('#verifiedAddressResult').empty();
            $element.find('#previouslySelectedResult').empty();

            if (verifiedAddress.length > 0) {

                $scope.loadFirstTime = false;

                $scope.verifiedAddress = verifiedAddress;
                // alert();

                $element.find('#unverifiedAddressResult').empty();
                $element.find('#verifiedAddressResult').empty();
                for (var i = 0; i < $scope.verifiedAddress.length; i++) {

                    var mainDiv = $('<div> </div>');
                    mainDiv.appendTo($element.find('#verifiedAddressResult'));

                    var id = "rdo" + i;

                    var radioBtn = $('<input style="float:left" type="radio" data-index="' + i + '" name="address" id="' + id + '" />');
                    var divs = $('<label for="' + id + '" >' + $scope.verifiedAddress[i].displayString() + '</label>');


                    radioBtn.click(function () {
                        var t = $(this).attr('data-index');
                        $scope.selectedStop.street1 = $scope.verifiedAddress[t].json.delivery_line_1;
                        $scope.selectedStop.zip = $scope.verifiedAddress[t].json.components.zipcode;
                        $scope.selectedStop.city = $scope.verifiedAddress[t].json.components.city_name;
                        $scope.selectedStop.state = $scope.verifiedAddress[t].json.components.state_abbreviation;
                        $scope.$apply();
                        //alert($scope.verifiedAddress[t].json.delivery_line_1);
                    });

                    radioBtn.appendTo($element.find(mainDiv));
                    divs.appendTo($element.find(mainDiv));
                    if (i == $scope.verifiedAddress.length - 1) {
                        break;
                    }
                }
            } else {



                $element.find('#unverifiedAddressResult').empty();
                $element.find('#verifiedAddressResult').empty();
                var mainDiv = $('<div style="width:100%"></div>');
                mainDiv.appendTo($element.find('#unverifiedAddressResult'));

                var id = "rdo" + i;

                var radioBtn = $('<input style="float:left" type="radio" name="address" id="' + id + '" />');
                var divs = $('<label for="' + id + '" >' + vals.street1 + " , " + vals.city + " , " + vals.state + " , " + vals.zip + '</label>');

                radioBtn.appendTo($element.find(mainDiv));
                divs.appendTo($element.find(mainDiv));

            }


            $scope.SetPreviousSelectedStop();


        });

    };

    $scope.ShowMap = function(address) {};


    $scope.SetPreviousSelectedStop = function () {


        if ($scope.selectedStop.address) {
            $element.find('#previouslySelectedResult').empty();
            var mainDiv = $('<div style="width:100%"></div>');
            mainDiv.appendTo($element.find('#previouslySelectedResult'));

            var id = "rdo1";
            var radioBtn = null;
            if ($scope.loadFirstTime == true) {

                radioBtn = $('<input style="float:left" type="radio" name="address" id="' + id + '" checked/>');
            }
            else {
                radioBtn = $('<input style="float:left" type="radio" name="address" id="' + id + '" />');
            }

            var divs = $('<label for="' + id + '" >' + $scope.selectedStop.address + '</label>');
            radioBtn.click(function () {

                var ty = $scope.selectedStop.address.split(',');
                $scope.selectedStop.street1 = ty[0];


                $scope.selectedStop.city = ty[1];

                var tt = ty[2].split(' ');
                $scope.selectedStop.state = tt[1];
                $scope.selectedStop.zip = tt[2];
                $scope.$apply();
            });


            radioBtn.appendTo($element.find(mainDiv));
            divs.appendTo($element.find(mainDiv));
        }

        

    };


};