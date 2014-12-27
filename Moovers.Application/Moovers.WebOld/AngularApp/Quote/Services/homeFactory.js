quoteApp.factory('homeFactory', ['$rootScope', '$http', '$q', homeFactory]);

function homeFactory($rootScope, $http, $q) {

    var serviceDefer = new Object();
    serviceDefer.URL = 'http://localhost:50959';


    serviceDefer.GetRecentQuotes = function() {
        
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/QuoteTodayJson/').
              success(function (data, status, headers, config) {

                  //serviceDefer.servicePromise = $q.defer();
                  test.resolve(data);
                  //return serviceDefer.servicePromise.promise;

              }).
              error(function (data, status, headers, config) {
                  test.reject(data);
                  //return serviceDefer.servicePromise.promise;
              });
        return test.promise;


    };



    serviceDefer.MovingToday = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/ScheduleTodayJson/').
              success(function (data, status, headers, config) {

                  //serviceDefer.servicePromise = $q.defer();
                  test.resolve(data);
                  //return serviceDefer.servicePromise.promise;

              }).
              error(function (data, status, headers, config) {
                  test.reject(data);
                  //return serviceDefer.servicePromise.promise;
              });
        return test.promise;

    };


    return serviceDefer;
};
