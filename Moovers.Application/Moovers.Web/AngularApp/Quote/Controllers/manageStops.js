quoteApp.controller('manageStops', ['quoteFactory','addressFactory','$timeout','$scope',manageStops]);

function manageStops(quoteFactory, addressFactory,$timeout,$scope) {

   
    $scope.selectedStop = {};
    $scope.allstates = '';
    
    $scope.ScopeDialogInitialize = function(id) {

        for (var i = 0; i < $scope.$parent.selectedQuote.Stops.length; i++) {
            if ($scope.$parent.selectedQuote.Stops[i].id == id) {
                $scope.selectedStop = $scope.$parent.selectedQuote.Stops[i];
            }
        }
        $timeout(function () {
            $scope.$apply();
        });
    };


 
    $scope.Clear = function() {

        $scope.selectedStop = {};

    };
    $scope.AddStop = function () {

        $scope.$parent.selectedQuote.Stops.push($scope.selectedStop);
        quoteFactory.UpdateStops($scope.$parent.selectedQuote.QuoteID, $scope.$parent.selectedQuote.Stops);
        quoteFactory.servicePromise.promise.then(function (stopsIdjson) {
            $scope.$parent.RefreshStops();//Init();
        });
    };

    $scope.DeleteStop = function() {

        var id = $scope.selectedStop.id;
        quoteFactory.DeleteStop(id);
        quoteFactory.servicePromise.promise.then(function (data) {
            //alert(data);
            if (data=='OK') {
                $scope.$parent.RefreshStops();
               
            }
        });

    };



};