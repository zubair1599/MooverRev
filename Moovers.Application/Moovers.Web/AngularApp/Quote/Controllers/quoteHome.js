quoteApp.controller('quoteHome', ['quoteFactory','addressFactory','inventoryFactory', '$scope','$window','$timeout', quoteHome]);


function quoteHome(quoteFactory,addressFactory,inventoryFactory, $scope, $window, $timeout) {

    //var contt = this;

    $scope.currentTab = 0;

    $scope.selectedQuote = {};
    $scope.selectedQuote.FranchiseLogo = '/static/img/logos/none.png';

    $scope.QuoteQuickLook = '';
    
    $scope.franchiseAddress = '';
    $scope.franchiseID = '';

    $scope.customerID = '';
    $scope.customer = null;
    $scope.DisplayName = '';

    $scope.DistanceFromLastStop = '';
    $scope.TimeFromLastStop = '';

    $scope.SERVER = $window.SERVER;
    $scope.Utility = $window.Utility;
    $scope.MinimumMovers = $window.MinimumMovers;

    $scope.AllRooms = [];

    $scope.AllInventories = [];


    $scope.InitInventory = function()
    {
        $scope.AllInventories = [];
        for (var i = 0; i < $scope.AllRooms.length; i++) {

            //itemrel.Item.Name
            //itemrel.Count
            var added = false;
            for (var j = 0; j < $scope.AllRooms[i].Items.length; j++) {

                var itemName = $scope.AllRooms[i].Items[j].Item.Name;
                var count = $scope.AllRooms[i].Items[j].Count;

                for (var k = 0; k < $scope.AllInventories.length; k++) {

                    if ($scope.AllInventories[k].Name === itemName) {
                        $scope.AllInventories[k].Count = $scope.AllInventories[k].Count + count;
                        added = true;
                        break;
                    }
                }
                if (!added) {
                    var item = new Object();
                    item.Name = itemName;
                    item.Count = count;
                    $scope.AllInventories.push(item);
                }
            }
        }

    };

    $scope.SetCustomer = function(json, id) {
        $scope.customer = json;
        $scope.DisplayName = $scope.customer.DisplayName;
        $scope.customerID = id;
        $timeout(function () {
            $scope.$apply();
        });
    };


    $scope.SetAllRooms = function() {
        
        $scope.AllRooms = [];
        for (var i = 0; i < $scope.selectedQuote.Stops.length; i++) {

            $scope.AllRooms = $scope.AllRooms.concat($scope.selectedQuote.Stops[i].rooms);

        }
        $scope.InitInventory();
        //$scope.$apply();

    };

    $scope.Init = function (quote) {

        var param = document.URL.substring(document.URL.indexOf('?') + 1);
       
        var lookup = param.substring(param.indexOf('=') + 1);
       
        if (lookup.indexOf('#')>-1) {
            lookup = lookup.substring(0, lookup.indexOf('#'));
        }
        if (typeof quote == 'undefined' && param.indexOf("lookup=") > -1 && lookup != undefined) {
            $scope.currentTab = 0;
            $scope.selectedQuote.Lookup = lookup;
            $scope.UpdateQuicklook();
            quoteFactory.GetRecentQuote(lookup).then(function (data) {
         
                $scope.selectedQuote = data.quote;
                
                //$scope.GetFranchise();
                $scope.SetAllRooms();
               
               
            });
        } else if (typeof quote !== 'undefined') {
            $scope.selectedQuote = quote;
            $scope.UpdateQuicklook();
            $scope.GetFranchise();
            $scope.SetAllRooms();
           
            //$scope.SetDistanceTimes();
        }              
    };


    $scope.SetQuote = function(json) {
        $scope.Init(json);
    };

    $scope.UpdateQuicklook = function() {
        quoteFactory.Quicklook($scope.selectedQuote.Lookup).then(function (quickLookdata) {
            $scope.QuoteQuickLook = quickLookdata;
        });
        //quoteFactory.servicePromise.promise.then(function(quickLookdata) {
        //    $scope.QuoteQuickLook = quickLookdata;
        //});

    };

    $scope.GetFranchise = function() {

        if ($scope.selectedQuote.Lookup != '') {
            quoteFactory.GetQuoteFranchise($scope.selectedQuote.Lookup).then(function(data) {
                $scope.franchiseAddress = data.franchiseAddress;
                $scope.franchiseID = data.franchiseID;
                $scope.SetDistanceTimes();
            });

        };

    };

    $scope.RefreshStops = function() {

        $scope.UpdateQuicklook();
        quoteFactory.GetAllStops($scope.selectedQuote.Lookup).then(function (data) {
            $scope.selectedQuote.Stops = (data);
            $scope.SetDistanceTimes();
            
            $scope.SetAllRooms();
        });
    

    };

    $scope.SetDistanceTimes = function () {
        if (typeof $scope.selectedQuote.Stops != 'undefined') {

            if ($scope.selectedQuote.Stops.length > 0) {

                for (var i = 0; i < $scope.selectedQuote.Stops.length; i++) {


                    (function (current) {

                        var backId = '';
                        var addressid = '';
                        if (current == 0) {
                            backId = $scope.franchiseID;
                            addressid = $scope.selectedQuote.Stops[current].addressid;
                        } else {
                            backId = $scope.selectedQuote.Stops[current - 1].addressid;
                            addressid = $scope.selectedQuote.Stops[current].addressid;
                        }
                        $scope.SetDistanceTimesForStop(current, backId, addressid);

                    })(i);
                }


                addressFactory.GetDistanceTime($scope.selectedQuote.Stops[$scope.selectedQuote.Stops.length - 1].addressid, $scope.franchiseID).then(function (data) {

                    $scope.DistanceFromLastStop = data.distance;
                    $scope.TimeFromLastStop = data.time;
                });


            }
        }

    };

    $scope.SetDistanceTimesForStop = function (index, backId, currentId) {

        addressFactory.GetDistanceTime(backId, currentId).then(function (data) {
            $scope.selectedQuote.Stops[index].distanceFromPrevious = data.distance;
            $scope.selectedQuote.Stops[index].timeFromPrevious = data.time;
        });

    };

    ////Initial functions
    $scope.Init();
};
    