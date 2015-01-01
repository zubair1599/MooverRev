quoteApp.controller('manageQuoteContact', ['quoteFactory', '$scope', '$element', '$window', '$timeout', manageQuoteContact]);

function manageQuoteContact(quoteFactory, $scope, $element, $window, $timeout) {


    $scope.searchQuery = '';
    $scope.searchResults = [];
    $scope.defaultSearch = false;
    $scope.selectedCustomer = '';
   

    $scope.GetCustomerFromQuote = function (lookup) {
        quoteFactory.GetCustomerFromQuote(lookup).then(function (data) {           
            $scope.selectedCustomer = data.primary;
            $scope.selectedShipper = data.secondary;
        });
    }

    $scope.SearchForCustomer = function (param) {
        $scope.searchResults = null;

        quoteFactory.GetPeopleJSONSearch(param).then(function (json) {

            $scope.searchResults = json;
            if ($scope.defaultSearch === true) {
                $scope.SetSelectedCustomer(json[1].AccountID);
                $scope.defaultSearch = false;
            }
        }, function (error) {
            alert("Error : customerSearch");

        });

    };

    $scope.SetSelectedCustomer = function (id) {

        quoteFactory.GetCustomerShortInformation(id).then(function (json) {

            $scope.selectedCustomer = json;

        }, function (error) {
            alert("Error : GetCustomerShortInformation");

        });

    };


    $scope.Init = function () {
        if ($scope.$parent.selectedQuote.Lookup !== undefined) {
            $scope.GetCustomerFromQuote($scope.$parent.selectedQuote.Lookup);
        }
        //$scope.defaultSearch = true;
        //$scope.SearchForCustomer('j')       
    }
    $scope.Init();
};