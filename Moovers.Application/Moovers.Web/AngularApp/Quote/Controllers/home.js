quoteApp.controller('home', ['homeFactory', '$timeout', '$scope','$element','$window', home]);

function home(homeFactory, $timeout, $scope,$element,$window) {

    $scope.Leads = [];
    $scope.canAddShipper = false;
    $scope.quoteLookupQuery = null;
    $scope.quoteLookupQueryResults = [];
    

    $scope.GetLeads = function () {
        homeFactory.GetLeads().then(function (data) {

            $scope.Leads = data;

        }, function (err) { alert("er"); });
    
    };


    $scope.SearchQuotes = function() {
        $scope.quoteLookupQueryResults = [];
        homeFactory.SearchByLookUp($scope.quoteLookupQuery).then(function (data) {

            $element.find('#searchResultsHeader').addClass('open');
            var arr = JSON.parse(data);
            //jQuery.each(arr, function (Franchise, value) {
            //    Franchise = "../"+value.substr(1, value.length);
            //});
            $scope.quoteLookupQueryResults = arr;
            $element.click(function() {
                $element.find('#searchResultsHeader').removeClass('open');
            });
            //$element.find('#searchResultsHeader').mouseout(function() {

            //    $element.find('#searchResultsHeader').removeClass('open');

            //});

        }, function(err) {
          
           
            

        });


    };

    $scope.QuoteNavigate = function(lookup) {
        

        $window.location.assign('/new/quote?lookup=' + lookup);
    };


    $scope.GetStats = function() {
        homeFactory.GetQuoteStats('').then(function (data) {
            $scope.QuotesStats.Open = [];
            $scope.QuotesStats.Scheduled = [];
            $scope.QuotesStats.Lost = [];
            $scope.QuotesStats.Won = [];
            $scope.QuotesStats.Open.SubStatuses = [];
            $scope.QuotesStats.Open.SubStatuses.Expired = [];
            $scope.QuotesStats.Open.SubStatuses.Unassigned = [];

            $scope.QuotesStats.Open.Count = data.OpenCount;
            $scope.QuotesStats.Open.Amount = data.OpenAmount;
            $scope.QuotesStats.Scheduled.Count = data.ScheduledCount;
            $scope.QuotesStats.Scheduled.Amount = data.ScheduledAmount;
            $scope.QuotesStats.Lost.Amount = data.Lost30DaysAmount;
            $scope.QuotesStats.Lost.Count = data.Lost30DaysCount;
            $scope.QuotesStats.Won.Count = data.Won30DaysCount;
            $scope.QuotesStats.Won.Amount = data.Won30DaysAmount;
            $scope.QuotesStats.Open.SubStatuses.Expired.Count = data.ExpiringCount;
            $scope.QuotesStats.Open.SubStatuses.Unassigned.Count = data.UnassignedCount;
            

        }, function(er) {});


    };
  
    $scope.Init = function() {
        
        $scope.GetLeads();                
    };

    $scope.Init();

};
