quoteApp.controller('quoteHome', ['quoteFactory','addressFactory','inventoryFactory', 'utilityFactory','$scope','$window','$timeout','$q', quoteHome]);


function quoteHome(quoteFactory,addressFactory,inventoryFactory,utilityFactory, $scope, $window, $timeout,$q) {

    $scope.currentTab = 0;
    $scope.canAddShipper= true;
    $scope.selectedQuote = {};
    $scope.selectedQuote.FranchiseLogo = '/static/img/logos/none.png';

    $scope.QuoteQuickLook = '';
    $scope.loadingQuickLook = false;
   

    $scope.customerID = '';
    $scope.customer = null;
    $scope.DisplayName = '';

    $scope.DistanceFromLastStop = '';
    $scope.TimeFromLastStop = '';

    $scope.SERVER = $window.SERVER;
    $scope.Utility = $window.Utility;
    $scope.MinimumMovers = $window.MinimumMovers;

   


   

    $scope.SetCustomer = function(json, id) {
        $scope.customer = json;
        $scope.DisplayName = $scope.customer.DisplayName;
        $scope.customerID = id;
        $timeout(function () {
            $scope.$apply();
        });
    };

   

    $scope.SetQuote = function(json) {
        $scope.Init(json);
    };

    $scope.UpdateQuicklook = function () {
        $scope.loadingQuickLook = true;
        quoteFactory.Quicklook($scope.selectedQuote.Lookup).then(function (quickLookdata) {
            $scope.QuoteQuickLook = quickLookdata;
            $scope.QuoteQuickLook.DriveDuration = utilityFactory.formatHours($scope.QuoteQuickLook.DriveDuration, true);
            $scope.QuoteQuickLook.TotalDuration = utilityFactory.getEstimateString($scope.QuoteQuickLook.TotalDuration);
            $scope.QuoteQuickLook.LaborDuration = utilityFactory.formatHours($scope.QuoteQuickLook.LaborDuration, 15, true);
            $scope.QuoteQuickLook.FinalPostedPrice = utilityFactory.formatCurrency($scope.QuoteQuickLook.FinalPostedPrice);
            $scope.QuoteQuickLook.Balance = utilityFactory.formatCurrency($scope.QuoteQuickLook.Balance);
            $scope.QuoteQuickLook.FinalPrice = utilityFactory.formatCurrency($scope.QuoteQuickLook.FinalPrice);
            $scope.QuoteQuickLook.OriginalPrice = utilityFactory.formatCurrency($scope.QuoteQuickLook.OriginalPrice);
            $scope.loadingQuickLook = false;
        });
      
    };

   

    
    $scope.RefreshStops = function () {
        var t = $q.defer();
        $scope.UpdateQuicklook();
        quoteFactory.GetAllStops($scope.selectedQuote.Lookup).then(function (data) {
            $scope.selectedQuote.Stops = (data);
            t.resolve();

        });

        return t.promise;
    };
   

    

    $scope.Init = function (quote) {

        var param = document.URL.substring(document.URL.indexOf('?') + 1);

        var lookup = param.substring(param.indexOf('=') + 1);

        if (lookup.indexOf('#') > -1) {
            lookup = lookup.substring(0, lookup.indexOf('#'));
        }
        if (typeof quote == 'undefined' && param.indexOf("lookup=") > -1 && lookup != undefined) {
            $scope.currentTab = 0;
            $scope.selectedQuote.Lookup = lookup;
            $scope.UpdateQuicklook();
            quoteFactory.GetRecentQuote(lookup).then(function (data) {

                $scope.selectedQuote = data.quote;


            });
        } else if (typeof quote !== 'undefined') {
            $scope.selectedQuote = quote;
            $scope.UpdateQuicklook();
            $scope.SetAllRooms();
        }
    };


    $scope.Init();
};
    