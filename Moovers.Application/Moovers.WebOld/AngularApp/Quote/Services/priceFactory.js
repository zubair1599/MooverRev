quoteApp.factory('priceFactory', ['$rootScope', '$http', '$q', '$resource', priceFactory]);
function priceFactory($rootScope, $http, $q, $resource) {

    var serviceDefer = new Object();
    serviceDefer.URL = 'http://localhost:50959';


    serviceDefer.GetPriceDetails = function (lookup) {
        //serviceDefer.servicePromise = $q.defer();

        var test = $q.defer();
        $http.get(serviceDefer.URL + '/Quote/PricingJson/?id='+lookup).success(function (priceData) {
            // serviceDefer.servicePromise.resolve(returnedData);
            test.resolve(priceData);

        }).
       error(function (data, status, headers, config) {
           //serviceDefer.servicePromise.reject();
           test.reject(data);
       });
        return test.promise;
    };


    serviceDefer.SetDiscountPriority = function(lookup , type , amount) {
        var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/DiscountPriority', { id: lookup, discountType: type, percent: amount }).success(function (a) {
            // serviceDefer.servicePromise.resolve(returnedData);
            test.resolve(a);

        }).
       error(function (data, status, headers, config) {
           //serviceDefer.servicePromise.reject();
           test.reject(data);
       });
        return test.promise;
    };



    serviceDefer.SaveGuranteed = function (tquoteID,adjustment,trucks,packingMaterials,valuationType,discountCouponCode,forcedStorage) {

     var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/SaveGuaranteed/', { quoteID: tquoteID, adjustment: adjustment, trucks: trucks, packingMaterials: packingMaterials, valuationType: valuationType, discountCouponCode: discountCouponCode, forcedStorage: forcedStorage }).success(function (a) {
            // serviceDefer.servicePromise.resolve(returnedData);
            test.resolve(a);

        }).
       error(function (data, status, headers, config) {
           //serviceDefer.servicePromise.reject();
           test.reject(data);
       });
        return test.promise;
    };

    serviceDefer.SaveHourly = function (tquoteID, numTrucks, crewSize, estimateTime, packingMaterials, additionalDestination, valuationType, forcedStorage) {

        var test = $q.defer();
        $http.post(serviceDefer.URL + '/Quote/SaveHourly/', { quoteID: tquoteID, numTrucks: numTrucks, crewSize: crewSize, estimateTime: estimateTime, packingMaterials: packingMaterials, additionalDestination: additionalDestination, valuationType: valuationType, forcedStorage: forcedStorage }).success(function (a) {
            // serviceDefer.servicePromise.resolve(returnedData);
            test.resolve(a);

        }).
       error(function (data, status, headers, config) {
           //serviceDefer.servicePromise.reject();
           test.reject(data);
       });
        return test.promise;
    };











    return serviceDefer;



};