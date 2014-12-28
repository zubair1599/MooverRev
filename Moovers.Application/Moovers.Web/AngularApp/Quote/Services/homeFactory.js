﻿quoteApp.factory('homeFactory', ['$rootScope', '$http', '$q','$location', homeFactory]);

function homeFactory($rootScope, $http, $q, $location) {

    var serviceDefer = new Object();
  
    var protpcol = $location.$$protocol + '://';
    var host = $location.host();
    var port = ':' + $location.port();
    var url = protpcol + host +port;
    serviceDefer.URL = url;
    serviceDefer.GetDashboardData = function () {
        
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/GetDashboardJson/').
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

    serviceDefer.GetLeadCount = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Lead/GetLeadCount/').
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
    serviceDefer.GetLeads = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Lead/LeadJson/').
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

    serviceDefer.GetJobsForUser = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Report/SalesProjectionJson/').
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

    serviceDefer.Surveys = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/SurveyTodayJson/').
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
    serviceDefer.Messages = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/MessagesJson/').
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
    serviceDefer.addNewMessage = function (data) {
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/AddMessage?addMessage='+data).
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
    }
    serviceDefer.removeMessage = function (Id) {
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Home/RemoveMsg?id=' + Id).
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
    }
    serviceDefer.GetQuoteStats = function (query) {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/GetStats/search=' + query).
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


    serviceDefer.GetStorageCount = function () {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Storage/StorageCount/').
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
