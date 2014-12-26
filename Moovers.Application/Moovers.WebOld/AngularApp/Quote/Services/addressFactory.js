﻿quoteApp.factory('addressFactory', ['$rootScope', '$http', '$q', '$resource',addressFactory]);

function addressFactory($rootScope, $http, $q, $resource) {
    
    var serviceDefer = new Object();
    serviceDefer.URL = 'http://localhost:50959';


    serviceDefer.GetStates = function () {
        serviceDefer.servicePromise = $q.defer();
        $http.get(serviceDefer.URL + '/Address/GetAllStates/').
              success(function (data, status, headers, config) {

                  //serviceDefer.servicePromise = $q.defer();
                  serviceDefer.servicePromise.resolve(data);

              }).
              error(function (data, status, headers, config) {
                  //serviceDefer.servicePromise = $q.defer();
                  serviceDefer.servicePromise.reject();
              });
      
    };


    serviceDefer.GetBuildingType = function (type) {
        serviceDefer.servicePromise = $q.defer();
        $http.get(serviceDefer.URL + '/Address/AddressTypes/?type='+type).
              success(function (data, status, headers, config) {

                  //serviceDefer.servicePromise = $q.defer();
                serviceDefer.servicePromise.resolve(data);
                //return serviceDefer.servicePromise.promise;

            }).
              error(function (data, status, headers, config) {
                  serviceDefer.servicePromise.reject(data);
                  //return serviceDefer.servicePromise.promise;
              });
        
    };

    serviceDefer.GetDistanceTime = function(id1, id2) {
        //serviceDefer.servicePromise = $q.defer();
        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Address/GetDistance/?address1ID=' + id1 + '&address2ID='+ id2).
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