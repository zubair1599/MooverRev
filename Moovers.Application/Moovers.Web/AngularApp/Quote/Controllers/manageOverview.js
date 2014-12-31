quoteApp.controller('manageOverview', ['$scope', mangeOverview]);

function mangeOverview($scope) {


    $scope.quoteStatus = [];



    $scope.Init = function() {

        $scope.InitStatus();


    };

    $scope.InitStatus = function () {

        $scope.quoteStatus = [];

        var created = $scope.$parent.selectedQuote.Created;

        var item = new Object();
        item.Name = "Created";
        item.Date = created;
        $scope.quoteStatus.push(item);

        for (var i = 0; i < $scope.$parent.selectedQuote.Schedules.length; i++) {
            
            item.Name = "Schedule";
            item.Date = moment($scope.$parent.selectedQuote.Schedules[i].Date).format("dddd, MMMM Do YYYY");
            $scope.quoteStatus.push(item);
        }



    };


    $scope.Init();

};
