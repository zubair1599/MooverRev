quoteApp.controller('home', ['homeFactory', '$timeout', '$scope', home]);

function home(homeFactory, $timeout, $scope) {


    $scope.Quotes = [];
    $scope.MovingToday = [];


    $scope.GetRecentQuote = function() {

        homeFactory.GetRecentQuotes().then(function(data) {

            $scope.Quotes = data;

        }, function(err) { alert("er"); });


    };
    $scope.MovingToday = function() {
        homeFactory.MovingToday().then(function (data) {

            $scope.MovingToday = data;

        }, function (err) { alert("er"); });

    };

    $scope.GetRecentQuote();
    $scope.MovingToday();

};
