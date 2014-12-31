quoteApp.controller('home', ['homeFactory', '$timeout', '$scope', home]);

function home(homeFactory, $timeout, $scope) {

    $scope.Leads = [];
    $scope.canAddShipper = false;
    $scope.GetLeads = function () {
        homeFactory.GetLeads().then(function (data) {

            $scope.Leads = data;

        }, function (err) { alert("er"); });
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
