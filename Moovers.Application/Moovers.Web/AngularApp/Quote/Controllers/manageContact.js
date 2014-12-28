quoteApp.controller('manageContact', ['quoteFactory','$scope','$element','$window','$timeout', manageContact]);

function manageContact(quoteFactory, $scope, $element, $window, $timeout) {


    $scope.searchQuery = '';
    $scope.searchResults = [];
    $scope.GetCustomerFromQuote = function (lookup) {
        quoteFactory.GetCustomerFromQuote(lookup).then(function (customerData) {
            $scope.selectedCustomer = customerData;
        });
    }

    $scope.SearchForCustomer = function () {
        $scope.searchResults = null;
        quoteFactory.GetPeopleJSONSearch($scope.searchQuery).then(function (json) {

            $scope.searchResults = json;
        }, function (error) {
            alert("Error : customerSearch");

        });

    };

    $scope.SetSelectedCustomer = function (id) {

        quoteFactory.GetCustomerShortInformation(id).then(function (json) {

            $scope.$parent.SetCustomer(json, id);



        }, function (error) {
            alert("Error : GetCustomerShortInformation");

        });

    };

    $scope.movedate = '';
    $scope.hear = '';

    $scope.StartQuote = function () {

        
            var id = $scope.$parent.customerID;
            var para = {

                shippingAccount: id,
                AccountID: id,
                movedate: $scope.movedate,
                referralmethod: $scope.hear

            };
            quoteFactory.AddQuote(para).then(function (json) {

                $scope.$parent.SetQuote(json.quote);

                // $location.path('/stops');


            });

        

    };
    $scope.Init = function () {
        $scope.GetCustomerFromQuote($scope.$parent.selectedQuote.Lookup);
    }
    $scope.Init();
};