quoteApp.factory('scheduleFactory', ['$rootScope', '$http', '$q', '$resource', scheduleFactory]);

function scheduleFactory($rootScope , $http , $q , $resource) {
  
    
    var serviceDefer = new Object();
    serviceDefer.URL = 'http://localhost:50600';

    serviceDefer.GetScheduleForQuote = function (id) {
        //serviceDefer.servicePromise = $q.defer();
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/ScheduleJson/?id=' + id).
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

    serviceDefer.GetQuotesPerDay = function(id , day, month, year) {
        
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/ScheduleDayJson/?id=' + id+ '&day='+day+'&month='+month+'&year='+year).
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
    serviceDefer.ScheduleJob = function(quoteId, model) {
        var promiseWrapper = $q.defer();

        $http.post(serviceDefer.URL + '/Quote/ScheduleJob/', {quoteId:quoteId , model : model}).
            success(function(data, status, headers, config) {

                //serviceDefer.servicePromise = $q.defer();
                promiseWrapper.resolve(data);
                //return serviceDefer.servicePromise.promise;

            }).
            error(function(data, status, headers, config) {
                promiseWrapper.reject(data);
                //return serviceDefer.servicePromise.promise;
            });
        return promiseWrapper.promise;
    };

    serviceDefer.GetScheduleForMonth = function (start , end , franchiseId) {
        var promiseWrapper = $q.defer();

        $http.get(serviceDefer.URL + '/Quote/GetSchedule1/?start=' + start + '&end=' + end + '&franchiseid='+franchiseId).
            success(function (data, status, headers, config) {

                //serviceDefer.servicePromise = $q.defer();
                promiseWrapper.resolve(data);
                //return serviceDefer.servicePromise.promise;

            }).
            error(function (data, status, headers, config) {
                promiseWrapper.reject(data);
                //return serviceDefer.servicePromise.promise;
            });
        return promiseWrapper.promise;
    };




    return serviceDefer;

};
