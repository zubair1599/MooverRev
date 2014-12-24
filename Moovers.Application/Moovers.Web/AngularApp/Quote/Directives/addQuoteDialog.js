quoteApp.directive('addQuoteDialog', ['quoteFactory','$location',Directive]);

function Directive(quoteFactory,$location) {

    return {

       scope: {},
       restrict: 'E',
        templateUrl: '/AngularApp/Quote/Templates/addQuoteDialog.html',
        controllerAs: 'raceResult',
        controller: function ($scope, quoteFactory,$location) {

            $scope.movedate = '';
            $scope.hear = '';
            $scope.ShipperName = function() { return $scope.$parent.DisplayName; };

            $scope.AddQuote = function () {
                var id = $scope.$parent.customerID;
                var para = {
                    
                    shippingAccount: id,
                    AccountID: id,
                    movedate: $scope.movedate,
                    referralmethod: $scope.hear

                };
                quoteFactory.AddQuote(para);
                quoteFactory.servicePromise.promise.then(function(json) {

                    $scope.$parent.SetQuote(json.quote);
                    
                   // $location.path('/stops');


                });

            };
        }

    };

};
