quoteApp.factory('inventoryFactory', ['$rootScope', '$http', '$q', '$resource', inventoryFactory]);

function inventoryFactory($rootScope, $http, $q, $resource) {

    var serviceDefer = new Object();
    serviceDefer.InventoryList = [];
    serviceDefer.searchItems = [];
    serviceDefer.Boxes = [];
    serviceDefer.CustomBoxes = [];

    serviceDefer.URL = 'http://localhost:50600';

    serviceDefer.UpdateInventory = function(quoteId , json) {

        var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/Inventory/', { quoteid: quoteId , rooms: json}).
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