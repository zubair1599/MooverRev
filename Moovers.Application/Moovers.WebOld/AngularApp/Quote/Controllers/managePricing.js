quoteApp.controller('managePricing', ['priceFactory','$scope', '$element', '$window', '$timeout', managePricing]);

function managePricing(priceFactory, $scope, $element, $window, $timeout) {

    $scope.PricingDetails = {};
    $scope.discountAmount = 0;
    $scope.discountType = 0;

    $scope.valuationID = '';
    $scope.valuation = '';

    $scope.discountP = 0;
    $scope.discountV = 0;
    $scope.crewArray = [2, 3];
    $scope.additionalFee = null;
    

    $scope.GetPriceDetails = function() {
        
        priceFactory.GetPriceDetails($scope.$parent.selectedQuote.Lookup).then(function (json) {

            //$scope.$parent.SetCustomer(json, id);
            
            var price = json;
            $scope.PricingDetails = price;
          
          
            $scope.PricingDetails.QuotePriceDetails.TotalTime= $scope.PricingDetails.QuotePriceDetails.TotalTime;

            $scope.PricingDetails.QuotePriceDetails.TotalTime = $window.Math.floor($scope.PricingDetails.QuotePriceDetails.TotalTime) + " - " + $window.Math.ceil($scope.PricingDetails.QuotePriceDetails.TotalTime) + " Hours ";
            $scope.$parent.selectedQuote.PricingDetails = $scope.PricingDetails;
            $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount = $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice;
            //$scope.valuation = $scope.PricingDetails.ReplacementValuationOptionsGuaranteed[1];
            if ($scope.PricingDetails.QuotePriceDetails.HourlyData) {

                if (!$scope.PricingDetails.QuotePriceDetails.HourlyData.CustomerTimeEstimate) {
                    $scope.PricingDetails.QuotePriceDetails.HourlyData.CustomerTimeEstimate = 1;
                }
                
                $scope.additionalFee = $scope.PricingDetails.QuotePriceDetails.PricingType === 'Hourly' ? $scope.PricingDetails.QuotePriceDetails.HourlyData.FirstHourPrice - $scope.PricingDetails.QuotePriceDetails.HourlyData.HourlyPrice - $scope.PricingDetails.QuotePriceDetails.GetDefaultDestinationFee() : '0';

                for (var i = $scope.PricingDetails.QuotePriceDetails.Trucks * 2; i <= $scope.PricingDetails.QuotePriceDetails.Trucks * 3; i++) {
                    

                    $scope.crewArray.push(parseInt(i));
                }

            }
            
            $scope.CalculateHourly();

        }, function (error) {
            alert("Error : manage pricing - getpricedetails");

        });


    };


    $scope.CalculateHourly = function() {


        
        $scope.personPrice = $scope.PricingDetails.QuotePriceDetails.PERSON_PRICE_MULTIPLIER;
        $scope.truckPrice = $scope.PricingDetails.QuotePriceDetails.TRUCK_PRICE_MULTIPLIER;
        $scope.personPriceDestination = $scope.PricingDetails.QuotePriceDetails.PERSON_DESTINATION_MULTIPLIER;
        $scope.truckPriceDestination = $scope.PricingDetails.QuotePriceDetails.TRUCK_DESTINATION_MULTIPLIER;
        $scope.currentHour = $scope.PricingDetails.QuotePriceDetails.CURRENT_HOUR;

        

        //$scope.PricingDetails.QuotePriceDetails.Trucks
        //$scope.PricingDetails.QuotePriceDetails.CrewSize
        //$scope.PricingDetails.QuotePriceDetails.HourlyData.CustomerTimeEstimate


        

        var perHour = ($scope.personPrice * $scope.PricingDetails.QuotePriceDetails.CrewSize) + ($scope.truckPrice * $scope.PricingDetails.QuotePriceDetails.Trucks);
        var firstHour = ($scope.personPriceDestination * $scope.PricingDetails.QuotePriceDetails.CrewSize) + ($scope.PricingDetails.QuotePriceDetails.Trucks * $scope.truckPriceDestination) + perHour;

        var additionalDestination =parseInt($scope.additionalFee);
        if (additionalDestination) {
            firstHour += additionalDestination;
        }

        //var valuation = parseFloat($("[name=valuationTypeHourly] :selected").data("cost"));

        var cost = (($scope.PricingDetails.QuotePriceDetails.HourlyData.CustomerTimeEstimate - 1) * perHour) + firstHour + 0;

        $scope.estimatedHourlyPrice = $window.Utility.formatCurrency(cost);
        $scope.estimatedTotal = $window.Utility.formatCurrency(cost);

        $scope.firstHour = $window.Utility.formatCurrency(firstHour);
        $scope.firstHourLinePrice = $window.Utility.formatCurrency(firstHour) + " first hour";


        $scope.extraHours = $window.Utility.formatCurrency(perHour);

        $scope.hourlyLinePrice = $window.Utility.formatCurrency(perHour) + " /hour";

       // $("#hourly-pricing-summary").show();
       // $("#guaranteed-pricing-summary").hide();


    };


    $scope.SetDiscountPriority = function(type,amount) {

        $scope.discountType = type;
        $scope.discountAmount = amount;
        $scope.ApplyAdjustments();


     
    };


    $scope.ChangeValuation = function() {

        //alert($scope.valuationID);
        for (var i = 0; i < $scope.PricingDetails.ReplacementValuationOptionsGuaranteed.length; i++) {
            if ($scope.PricingDetails.ReplacementValuationOptionsGuaranteed[i].ValuationTypeID == $scope.valuationID) {
                $scope.valuation = $scope.PricingDetails.ReplacementValuationOptionsGuaranteed[i];
                
                $scope.ApplyAdjustments();

                
                break;
            }
        }
        


    };


    $scope.ApplyAdjustments = function() {



        $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount = $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice;
        if ($scope.discountType === 'value') {



            $scope.discountP = ( $scope.finaldiscountAmount / $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice) * 100;



            $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount = $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice + $scope.discountAmount;


        } else if ($scope.discountType === 'percent') {


            var tmp = $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice / 100;
            tmp = tmp * $scope.discountAmount;
            $scope.discountV = tmp;
            $scope.finaldiscountAmount = tmp;
            $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount = $scope.PricingDetails.QuotePriceDetails.GuaranteedPrice + tmp;
        }
        if ($scope.valuation!= '') {
            $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount = $scope.PricingDetails.QuotePriceDetails.GuaranteeData.GuaranteedPriceDiscount + $scope.valuation.Cost;


        }

    };


    $scope.SaveGuranteed = function() {


       
        var tquoteID = $scope.$parent.selectedQuote.QuoteID;
        // var adjustment = parseInt(-$scope.discountAmount);
        var adjustment = parseInt($scope.finaldiscountAmount);
        var trucks = $scope.PricingDetails.QuotePriceDetails.PricingTrucks;
        var packingMaterials = null;
        var valuationType = $scope.valuationID;
        var discountCouponCode = '';
        var forcedStorage = null;
        var tmp = 0;
        if ($scope.discountType=='percent') {
            tmp = $scope.discountP;
        } else {
            
        }
        priceFactory.SetDiscountPriority($scope.$parent.selectedQuote.Lookup, $scope.discountType, tmp).then(function (json) {
            },function(error) {
            });

        priceFactory.SaveGuranteed(tquoteID, adjustment, trucks, packingMaterials, valuationType, discountCouponCode, forcedStorage).then(function(j) {
            
            $scope.$parent.UpdateQuicklook();
        }, function(err) {

            
        });


    };


    $scope.SaveHourly = function() {
        
        

        priceFactory.SaveHourly($scope.$parent.selectedQuote.QuoteID,$scope.PricingDetails.QuotePriceDetails.Trucks,$scope.PricingDetails.QuotePriceDetails.CrewSize
            , $scope.PricingDetails.QuotePriceDetails.HourlyData.CustomerTimeEstimate, '', ''
            , $scope.PricingDetails.ReplacementValuationOptionsGuaranteed[1].ValuationTypeID).then(function (j) {

            $scope.$parent.UpdateQuicklook();
        }, function (err) {


        });
    };


    $scope.HourlyTruckChange = function() {


       

       
        
            $scope.crewArray = [];

            for (var i = $scope.PricingDetails.QuotePriceDetails.Trucks*2; i <= $scope.PricingDetails.QuotePriceDetails.Trucks*3; i++) {
                if (i == $scope.PricingDetails.QuotePriceDetails.Trucks*2) {
                    $scope.PricingDetails.QuotePriceDetails.CrewSize = parseInt(i);
                }

                $scope.crewArray.push(parseInt(i));
            }
        
        
        $scope.CalculateHourly();




    };


    $scope.HourlyCrewChange = function() {
        $scope.CalculateHourly();
    };


    $scope.HourlyEstimatedChange = function() {
        
        $scope.CalculateHourly();



    };

    $timeout(function() {

        $scope.GetPriceDetails();
        
        $scope.Init();

    },2000);
    


};