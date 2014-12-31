quoteApp.controller('manageContact', ['accountFactory','addressFactory','quoteFactory','$scope','$element','$window','$timeout', manageContact]);

function manageContact(accountFactory,addressFactory, quoteFactory, $scope, $element, $window, $timeout) {


    $scope.searchQuery = '';
    $scope.searchResults = [];
    $scope.defaultSearch = false;
    $scope.selectedCustomer = '';
    $scope.movedate = new Date();
    $scope.hear = '';
    $scope.estimatedDate = new Date();
    $scope.statesCodes = [];
    $scope.states = [];
    $scope.newBusinessAccount = new Object();
    $scope.newPersonAccount = new Object();
    $scope.newAccountType = '';

    $scope.GetCustomerFromQuote = function(lookup) {
        quoteFactory.GetCustomerFromQuote(lookup).then(function(customerData) {
            $scope.selectedCustomer = customerData;
        });
    };

    $scope.InitNewAccount = function(type) {

        if (type=='person') {
            $scope.newAccount = $scope.newPersonAccount;
        } else {
            $scope.newAccount = $scope.newBusinessAccount;
        }
        $scope.newAccountType = type;
        //$scope.newAccount = new Object();
        //$scope.newAccount.AccountType = type;
        //$scope.newAccount.Address = new Object();
        //$scope.newAccount.Address.MailingAddress = new Object();
        //$scope.newAccount.Address.BillingAddress = new Object();
        //$scope.newAccount.PrimaryPhone = new Object();
        //$scope.newAccount.SecondaryPhone = new Object();
        //$scope.newAccount.Fax = new Object();

    };

    $scope.SaveAccount = function() {

        accountFactory.AddAccount(null, $scope.newAccount, $scope.newAccountType).then(function (data) {
            $scope.selectedCustomer = data;
            //alert(data);


        }, function(err) {


        });


    };

    $scope.NewBusiness = function () {

        accountFactory.NewBusinessObject().then(function (data) {

            $scope.newBusinessAccount = JSON.parse(data);
            delete $scope.newBusinessAccount.Account.Lookup;
            //$scope.newAccount = $scope.newBusinessAccount; // remove this and bind type on Initnew account by type
        }, function (err) {


        });


    };

    //NewPersonObject
    $scope.NewPerson = function () {

        accountFactory.NewPersonObject().then(function (data) {

            $scope.newPersonAccount = JSON.parse(data);
            delete $scope.newPersonAccount.Account.Lookup;
            //$scope.newAccount = $scope.newBusinessAccount; // remove this and bind type on Initnew account by type
        }, function (err) {


        });


    };

    $scope.GetStates = function() {
        addressFactory.GetStates().then(function(states) {
            $.each(JSON.parse(states), function(index, fb) {

                $scope.statesCodes = $scope.statesCodes.concat(fb);
                $scope.states = $scope.states.concat(index);
            });
        });
    };


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
        $scope.NewBusiness();
        $scope.defaultSearch = true;
        $scope.SearchForCustomer('j');
        $scope.GetStates();
    }
    $scope.Init();
};