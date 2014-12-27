quoteApp.directive('customerSearchDialog', ['quoteFactory', CustomerSearch]);


function CustomerSearch(quoteFactory) {

    return {
        scope: {},
        restrict: 'E',
        templateUrl: '/AngularApp/Quote/Templates/customerSearchDialog.html',
        controllerAs: 'customerSearchController',
        controller: function ($scope, quoteFactory, $element) {

            $scope.searchQuery = '';
            $scope.searchResults = [];

            $scope.searchForCustomer = function () {
                $scope.searchResults = null;
                quoteFactory.GetPeopleJSONSearch($scope.searchQuery);
                quoteFactory.servicePromise.promise.then(function (json) {

                    $scope.searchResults = json;
                }, function(error) {
                    alert("Error : customerSearch");

                });

            };

            $scope.setSelectedCustomer = function(id) {

                quoteFactory.GetCustomerShortInformation(id);
                quoteFactory.servicePromise.promise.then(function (json) {

                    $scope.$parent.SetCustomer(json, id);
                    
                    

                }, function (error) {
                    alert("Error : GetCustomerShortInformation");

                });

            };

        },
        link: function ($scope, element, attrs) {

            //element.modal();
            var ty = element.children();
            element.find('#search-contacts').hide();


            element.find('.tab-icon-search').bind('click', function(e) {
                
                element.find('#account-contacts').hide();
                element.find('#search-contacts').show();
                element.find('.tab-icon').removeClass('active');
                
                element.find('.tab-icon-search').addClass('active');
            });

            element.find('.tab-icon-contact').bind('click', function (e) {
                element.find('#account-contacts').show();
                
                element.find('#search-contacts').hide();
                element.find('.tab-icon').removeClass('active');
                element.find('.tab-icon-search').addClass('active');
            });

            element.find('.tab-icon-search').bind('click', function(e) {
                

                element.find('.contact-list-container').addClass('show');

            });
         
            
            
        }

    };

};