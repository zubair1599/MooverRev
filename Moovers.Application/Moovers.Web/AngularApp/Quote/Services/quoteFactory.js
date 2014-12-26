quoteApp.factory('quoteFactory', ['$rootScope', '$http', '$q', quoteFactory]);

function quoteFactory($rootScope, $http, $q) {

    var serviceDefer = new Object();
    serviceDefer.servicePromise = '';
    serviceDefer.searchResults = [];
    serviceDefer.URL = 'http://localhost:50600';

    serviceDefer.GetPeopleJSON = function (searchQuery) {

        var take = 50;

        if (searchQuery=='') {
            var i = 0;
            while (i < 10) {

                serviceDefer.customerJson = '';

                serviceDefer.servicePromise = $q.defer();

                $http.get(serviceDefer.URL + '/Accounts/All?q=' + searchQuery + '&page=' + i + '&take=' + take).success(function (customerJson) {
                    serviceDefer.searchResults = serviceDefer.searchResults.concat(customerJson);
                    //serviceDefer.servicePromise.resolve(customerJson);
                    

                    }).
                          error(function (data, status, headers, config) {
                              serviceDefer.servicePromise.reject();
                });

                i++;
            }

            
        } 

    };

    serviceDefer.GetPeopleJSONSearch = function (searchQuery) {

        var take = 50;
            var i = 0;
        serviceDefer.customerJson = '';
        var test  = $q.defer();
        $http.get( serviceDefer.URL+ '/Accounts/All?q=' + searchQuery + '&page=' + i + '&take=' + take).success(function (customerJson) {
                    serviceDefer.searchResults = serviceDefer.searchResults.concat(customerJson);
                    test.resolve(customerJson);


        }).
        error(function (data, status, headers, config) {
            test.reject();
        });
        return test.promise;
    };

    serviceDefer.GetCustomerShortInformation = function(accountId) {

        var test = $q.defer();

        serviceDefer.servicePromise = $q.defer();
        $http.get(serviceDefer.URL + '/Accounts/Get/'+accountId).success(function(returnedData) {
            test.resolve(returnedData);

        }).
        error(function (data, status, headers, config) {
            test.reject();
        });

        return test.promise;
    };
    
    serviceDefer.AddQuote = function(quoteBasicInfo) {
        var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/AddQuoteJson/', {
            shippingAccount: quoteBasicInfo.shippingAccount,
            hiddenAccountID: quoteBasicInfo.AccountID,
            movedate: quoteBasicInfo.movedate,
            referralmethod: quoteBasicInfo.referralmethod,
            coll: null,
            personModel: null,
            businessModel: null
                
            }).
              success(function (data, status, headers, config) {
                  test.resolve(data);
              }).
              error(function (data, status, headers, config) {
                  test.reject();
              });
        return test.promise;
    };


    serviceDefer.Quicklook = function (quoteId) {
        var test = $q.defer();
        serviceDefer.servicePromise = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/GetQuickLook/', {quoteid : quoteId}).
              success(function (data, status, headers, config) {
                  test.resolve(data);
               
                  serviceDefer.servicePromise.resolve(data);
              }).
              error(function (data, status, headers, config) {
                  //test.reject(data);
                  
                  serviceDefer.servicePromise.reject();
              });
            return test.promise;
    };

    serviceDefer.GetQuoteFranchise = function(quoteId) {
        
        serviceDefer.servicePromise = $q.defer();
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/FranchiseDetails/?id='+ quoteId ).
              success(function (data, status, headers, config) {
                  test.resolve(data);
                  serviceDefer.servicePromise.resolve(data);
              }).
              error(function (data, status, headers, config) {
                  serviceDefer.servicePromise.reject();
              });
        return test.promise;
    };


    serviceDefer.GetAllStops = function (quoteId) {

        serviceDefer.servicePromise = $q.defer();
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/StopsJson/?id=' + quoteId).
              success(function (data, status, headers, config) {
                  test.resolve(data);
                  serviceDefer.servicePromise.resolve(data);
              }).
              error(function (data, status, headers, config) {
                  serviceDefer.servicePromise.reject();
              });
        return test.promise;
    };

    serviceDefer.AddressSuggestions = function(address) {
        serviceDefer.servicePromise = $q.defer();
        $http.post(serviceDefer.URL + '/Address/GetSuggestions/', {
            address: address

        }).
              success(function (data, status, headers, config) {

                  var verifiedaddresses = _.map(data, function (verified) {
                      return {
                          verified: true,
                          json: JSON.parse(JSON.stringify(verified)),
                          displayString: function () {
                              return verified.delivery_line_1 + ", " + verified.last_line;
                          }
                      };
                  });
                  serviceDefer.servicePromise.resolve(verifiedaddresses);
              }).
              error(function (data, status, headers, config) {
                  serviceDefer.servicePromise.reject();
              });

    };

    serviceDefer.UpdateStops = function (id, json) {
        var stopsjson = JSON.stringify((json));
        serviceDefer.servicePromise = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/Stops/', {
            quoteid: id,
            stopsjson : stopsjson

        }).
              success(function (data, status, headers, config) {

                 
                  serviceDefer.servicePromise.resolve();
              }).
              error(function (data, status, headers, config) {
                  serviceDefer.servicePromise.reject();
              });

    };

    serviceDefer.DeleteStop = function(id) {
        serviceDefer.servicePromise = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/DeleteStopJSON/?idd=' + id).success(function (returnedData) {
            serviceDefer.servicePromise.resolve(returnedData);

        }).
       error(function (data, status, headers, config) {
           serviceDefer.servicePromise.reject();
       });

    };

    serviceDefer.GetRecentQuote = function (lookup) {
        //serviceDefer.servicePromise = $q.defer();

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/GetRecentQuoteJson?lookup='+lookup).success(function (returnedData) {
            // serviceDefer.servicePromise.resolve(returnedData);
                test.resolve(returnedData);

            }).
       error(function (data, status, headers, config) {
           //serviceDefer.servicePromise.reject();
           test.reject(data);
       });
        return test.promise;
    };
    return serviceDefer;

};