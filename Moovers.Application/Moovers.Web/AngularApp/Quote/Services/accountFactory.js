quoteApp.factory('accountFactory', ['$location', '$http', '$q', utilityFactory]);
 
function utilityFactory($location,$http,$q) {
    
    var serviceDefer = new Object();
    var protpcol = $location.$$protocol + '://';
    var host = $location.host();
    var port = ':' + $location.port();
    var url = protpcol + host + port;
    serviceDefer.URL = url;
    var businessURL = '/Accounts/CreateBusiness2/';
    var personURL = '/Accounts/CreatePerson2/';


    serviceDefer.AddAccount = function (accountId, account,type) {
        var promiseWrapper = $q.defer();
        var typeurl = null;
        // var jsonStr = JSON.stringify(account);
        if (type=='person') {
            typeurl = personURL;
        } else {
            typeurl = businessURL;
        }

        $http.post(serviceDefer.URL + typeurl, { accountid: accountId, model: account }).
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

    


    serviceDefer.NewBusinessObject = function() {
        

        var promiseWrapper = $q.defer();
       
        $http.get(serviceDefer.URL + '/Accounts/CreateNewBusiness/').
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

    serviceDefer.NewPersonObject = function () {


        var promiseWrapper = $q.defer();

        $http.get(serviceDefer.URL + '/Accounts/CreateNewPerson/').
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