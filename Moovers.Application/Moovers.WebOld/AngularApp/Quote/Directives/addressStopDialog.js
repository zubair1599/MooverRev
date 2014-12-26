quoteApp.directive('stopDialog', ['quoteFactory', 'addressFactory', '$window', '$q','$timeout', addressSearchDialog]);

function addressSearchDialog(quoteFactory, addressFactory, $window, $q,$timeout) {
    return {
        scope: {},
        restrict: 'EA',

        controllerAs: 'stopController',
        templateUrl: '/AngularApp/Quote/Templates/addressStopDialog.html',
        controller: function ($scope,$element) {

            $scope.statesCodes = [];
            $scope.states = [];
            $scope.SearchAddress = '';
            $scope.current = 1;
            $scope.BuildingType = ['Residential', 'Commercial', 'Government'];
            $scope.Buildings = [];
            $scope.BuildingsCodes = [];
            $scope.stop = function () {
                return $scope.$parent.selectedStop;
            };


            //Reading States
            addressFactory.GetStates();
            addressFactory.servicePromise.promise.then(function(states) {
                $.each(JSON.parse(states), function (i, fb) {
                 
                    $scope.statesCodes = $scope.statesCodes.concat(fb);
                    $scope.states = $scope.states.concat(i);
                });
                $timeout(function() {
                    
                    addressFactory.GetBuildingType($scope.BuildingType[0]);
                    addressFactory.servicePromise.promise.then(function (buildingsz) {
                        var tmp = JSON.parse(buildingsz.buildings);
                        $scope.Buildings[0] = [];
                        $scope.BuildingsCodes[0] = [];

                        $scope.Buildings[1] = [];
                        $scope.BuildingsCodes[1] = [];

                        $scope.Buildings[2] = [];
                        $scope.BuildingsCodes[2] = [];


                        $.each((tmp), function (i, fb) {
                         
                            $scope.Buildings[0] = $scope.Buildings[0].concat(fb);
                            $scope.BuildingsCodes[0] = $scope.BuildingsCodes[0].concat(i);
                        });

                    

                        $timeout(function() {
                            addressFactory.GetBuildingType($scope.BuildingType[1]);
                            addressFactory.servicePromise.promise.then(function(buildingso) {
                                tmp = JSON.parse(buildingso.buildings);
                                $.each((tmp), function (i, fb) {
                                 
                                    $scope.Buildings[1] = $scope.Buildings[1].concat(fb);
                                    $scope.BuildingsCodes[1] = $scope.BuildingsCodes[1].concat(i);
                                });

                                $timeout(function () {
                                    addressFactory.GetBuildingType($scope.BuildingType[2]);
                                    addressFactory.servicePromise.promise.then(function (buildingst) {
                                     
                                        tmp = JSON.parse(buildingst.buildings);
                                        $.each((tmp), function (i, fb) {
                                      
                                            $scope.Buildings[2] = $scope.Buildings[2].concat(fb);
                                            $scope.BuildingsCodes[2] = $scope.BuildingsCodes[2].concat(i);
                                        });
                                    });
                                });

                            });
                        });


                    });
                });
                //$scope.$apply();
            });

            //Reading apartments
            
            


            
           

            $scope.FindAddress = function() {
                
                var address = new Object();
                address.street1 = $scope.stop().street1;
                address.street2 = $scope.stop().street2;
                address.city = $scope.stop().city;
                address.state = $scope.stop().state;
                address.zip = $scope.stop().zip;
                if (address.street1) {
                    var uri = encodeURI(address.display.replace(/[#?$+,\/:&;=@%{}|\\\[\]\^~]/g, ""));
                    var url = "http://maps.googleapis.com/maps/api/streetview?size=300x250&location=" + uri + "&sensor=false";
                    element.find('#mapImg').attr("src", url);

                }

                quoteFactory.AddressSuggestions(address);
                quoteFactory.servicePromise.promise.then(function (verifiedAddress) {


                    $element.find('#unverifiedAddressResult').empty();
                    $element.find('#verifiedAddressResult').empty();
                    $element.find('#previouslySelectedResult').empty();

                    if (verifiedAddress.length > 0) {

                        $scope.verifiedAddress = verifiedAddress;
                        // alert();

                       
                        for (var i = 0; i < $scope.verifiedAddress.length; i++) {

                            var mainDiv = $('<div> </div>');
                            mainDiv.appendTo($element.find('#verifiedAddressResult'));
                            var id = "rdo" + i;
                            var radioBtn = $('<input style="float:left" type="radio" onclick="alert();" name="' + id + '" />');
                            ;
                            var divs = $('<label for="' + id + '" >' + $scope.verifiedAddress[i].displayString() + '</label>');
                            radioBtn.live('click', function() { alert(); });
                            radioBtn.appendTo($element.find(mainDiv));
                            divs.appendTo($element.find(mainDiv));

                           
                           


                        }

                    } else {
                      
                        var mainDiv = $('<div style="width:100%"></div>');
                        mainDiv.appendTo($element.find('#unverifiedAddressResult'));

                        var id = "rdo" + i;

                        var radioBtn = $('<input style="float:left" id="unverifiedAddress" type="radio" name="' + id + '" />');
                        var divs = $('<label for="' + id + '" >' + address.street1 + " , " + " , " + address.street2 + ", " + address.city + " , " + address.state + " , " + address.zip + '</label>');
                        radioBtn.appendTo($element.find(mainDiv));
                        divs.appendTo($element.find(mainDiv));


                    }
                    element.find('#previouslySelectedResult').empty();
                    var mainDiv = $('<div style="width:100%"></div>');
                    mainDiv.appendTo(element.find('#previouslySelectedResult'));

                    var id = "rdo" + i;

                    var radioBtn = $('<input style="float:left" type="radio" name="' + id + '" />');
                    var divs = $('<label for="' + id + '" >' + $scope.stop().address + '</label>');
                    radioBtn.click(function () {

                        var ty = $scope.stop().address.split(',');
                        $scope.stop().street1 = ty[0];


                        $scope.stop().city = ty[1];

                        var tt = ty[2].split(' ');
                        $scope.stop().state = tt[1];
                        $scope.stop().zip = tt[2];
                        $scope.$apply();
                    });




                });

            };

        },
        link: function ($scope, element, attrs) {
            

            var options = {
                componentRestrictions: { country: "us" }
            };

            var input = element.find('#searchTextField')[0];
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

                    $scope.stop().street1 = vals.street1;
                    $scope.stop().city = vals.city;
                    $scope.stop().state = vals.state;
                    $scope.stop().zip = vals.zip;

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
                        var url = "http://maps.googleapis.com/maps/api/streetview?size=300x250&location=" + uri + "&sensor=false";
                        element.find('#mapImg').attr("src", url);

                    }



                    quoteFactory.AddressSuggestions(address);
                    quoteFactory.servicePromise.promise.then(function (verifiedAddress) {



                     


                        if (verifiedAddress.length>0) {
                        
                            $scope.verifiedAddress = verifiedAddress;
                            // alert();

                            element.find('#unverifiedAddressResult').empty();
                            element.find('#verifiedAddressResult').empty();
                            for (var i = 0; i < $scope.verifiedAddress.length; i++) {

                                var mainDiv = $('<div> </div>');
                                mainDiv.appendTo(element.find('#verifiedAddressResult'));

                                var id = "rdo" + i;

                                var radioBtn = $('<input style="float:left" type="radio" data-index="'+i+'" name="' + id + '" />');
                                var divs = $('<label for="' + id + '" >' + $scope.verifiedAddress[i].displayString() + '</label>');


                                radioBtn.click(function() {
                                    var t = $(this).attr('data-index');
                                    $scope.stop().street1 = $scope.verifiedAddress[t].json.delivery_line_1;
                                    $scope.stop().zip = $scope.verifiedAddress[t].json.components.zipcode;
                                    $scope.stop().city = $scope.verifiedAddress[t].json.components.city_name;
                                    $scope.stop().state = $scope.verifiedAddress[t].json.components.state_abbreviation;
                                    $scope.$apply();
                                    //alert($scope.verifiedAddress[t].json.delivery_line_1);
                                });
                                
                                radioBtn.appendTo(element.find(mainDiv));
                                divs.appendTo(element.find(mainDiv));
                                if (i == $scope.verifiedAddress.length - 1) {
                                    break;
                                }
                            }
                        } else {



                            element.find('#unverifiedAddressResult').empty();
                            element.find('#verifiedAddressResult').empty();
                            var mainDiv = $('<div style="width:100%"></div>');
                            mainDiv.appendTo(element.find('#unverifiedAddressResult'));

                            var id = "rdo" + i;

                            var radioBtn = $('<input style="float:left" type="radio" name="' + id + '" />');
                            var divs = $('<label for="' + id + '" >' + vals.street1 + " , " + vals.city + " , "+ vals.state + " , "+ vals.zip + '</label>');

                            radioBtn.appendTo(element.find(mainDiv));
                            divs.appendTo(element.find(mainDiv));

                        }

                        element.find('#previouslySelectedResult').empty();
                        var mainDiv = $('<div style="width:100%"></div>');
                        mainDiv.appendTo(element.find('#previouslySelectedResult'));

                        var id = "rdo" + i;

                        var radioBtn = $('<input style="float:left" type="radio" name="' + id + '" />');
                        var divs = $('<label for="' + id + '" >' + $scope.stop().address + '</label>');
                        radioBtn.click(function() {

                            var ty = $scope.stop().address.split(',');
                            $scope.stop().street1 = ty[0];
                            
                            
                            $scope.stop().city = ty[1];
                            
                            var tt = ty[2].split(' ');
                            $scope.stop().state = tt[1];
                            $scope.stop().zip = tt[2];
                            $scope.$apply();
                        });


                        radioBtn.appendTo(element.find(mainDiv));
                        divs.appendTo(element.find(mainDiv));


                        

                    });

                    

                  

                    //Utility.objectToForm(vals, Stops.elements.stopModal.find("form.addr-search"));
                    if (vals.zip) {
                       // Stops.elements.stopModal.find("form.addr-search").find("[type=submit]").click();
                    }
                } else if (place.name) {
                    element.find('#street1').val(place.name).focus();
                    //Stops.elements.stopModal.find("form.addr-search [name=street1]").val(place.name).focus();

                }
            });

        }

    };
    


}