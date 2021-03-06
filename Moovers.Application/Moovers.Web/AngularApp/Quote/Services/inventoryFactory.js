﻿quoteApp.factory('inventoryFactory', ['$rootScope', '$http', '$q', '$resource', '$location', inventoryFactory]);

function inventoryFactory($rootScope, $http, $q, $resource,$location) {

    var serviceDefer = new Object();
    serviceDefer.InventoryList = [];
    serviceDefer.searchItems = [];
    serviceDefer.Boxes = [];
    serviceDefer.CustomBoxes = [];
     
    var serviceDefer = new Object();
    var protpcol = $location.$$protocol + '://';
    var host = $location.host();
    var port = ':' + $location.port();


    var url = protpcol + host + port;


    serviceDefer.URL = url;

    serviceDefer.UpdateInventory = function(quoteId , json) {

        var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/Inventory/', { quoteid: quoteId, rooms: json },{       
            ignoreLoadingBar: true
            }

            ).
             success(function (data, status, headers, config) {
                 test.resolve(data);

                 
             }).
             error(function (data, status, headers, config) {
                 test.reject(data);

                 
             });
        return test.promise;

    };


    serviceDefer.GetInventoryItems = function (quoteId) {

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/InventoryItems/?lookup='+quoteId).
             success(function (data, status, headers, config) {
                
                 serviceDefer.InventoryList = JSON.parse(data.items);

                 serviceDefer.searchItems = $.map(serviceDefer.InventoryList, function (item) {
                     return item.Name;
                 });
                 serviceDefer.Boxes = JSON.parse(data.boxes);
                 serviceDefer.CustomBoxes = JSON.parse(data.customitems);
                    test.resolve();
             }).
             error(function (data, status, headers, config) {
                 test.reject(data);


             });
        return test.promise;

    };


    return serviceDefer;

};