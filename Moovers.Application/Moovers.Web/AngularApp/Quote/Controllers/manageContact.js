quoteApp.controller('manageContact', ['quoteFactory','$scope','$element','$window','$timeout', manageContact]);

function manageContact(quoteFactory, $scope, $element, $window, $timeout) {


    $scope.searchQuery = '';
    $scope.searchResults = [];
    $scope.defaultSearch = false;
    $scope.selectedCustomer = '';
    $scope.movedate = new Date();
    $scope.hear = '';
    $scope.estimatedDate = new Date();
    

    $scope.GetCustomerFromQuote = function (lookup) {
        quoteFactory.GetCustomerFromQuote(lookup).then(function (customerData) {
            $scope.selectedCustomer = customerData;
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

    $scope.AddNewQuote = function () {

        alert('Added');
    }

    $scope.StartQuote = function () {

        
            var id = $scope.$parent.customerID;
            var para = {

                shippingAccount: $scope.selectedCustomer.AccountID,
                AccountID: $scope.selectedCustomer.AccountID,
                movedate: $scope.movedate,
                referralmethod: $scope.hear

            };
            quoteFactory.AddQuote(para).then(function (json) {

               // $scope.$parent.SetQuote(json.quote);
                window.location.assign('/new/quote?lookup=' + json.lookup);
               // $location.path('/quote');


            });

        

    };
    $scope.Init = function () {
        //if ($scope.$parent.selectedQuote.Lookup !== undefined) {
        //    $scope.GetCustomerFromQuote($scope.$parent.selectedQuote.Lookup);
        //}
        $scope.defaultSearch = true;
        $scope.SearchForCustomer('')
       
    }
    $scope.Init();
};