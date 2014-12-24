quoteApp.controller('home', ['homeFactory', '$timeout', '$scope', home]);

function home(homeFactory, $timeout, $scope) {


    $scope.RecentQuotes = [];
    $scope.Surveys = [];
    $scope.Messages = [];
    $scope.MovingToday = [];
    $scope.QuotesStats = [];
    $scope.LeadCount = 0;
    $scope.StorageCount = 0;
    $scope.Jobs = [];

    $scope.GetRecentQuote = function() {

        homeFactory.GetRecentQuotes().then(function(data) {

            $scope.RecentQuotes = data;

        }, function(err) { alert("er"); });


    };
    $scope.GetSurveys = function () {

        homeFactory.Surveys().then(function (data) {

            $scope.Surveys = data;

        }, function (err) { alert("er"); });


    };
    $scope.GetMessages = function () {

        homeFactory.Messages().then(function (data) {

            $scope.Messages = data;

        }, function (err) { alert("er"); });


    };
    $scope.MovingToday = function() {
        homeFactory.MovingToday().then(function(data) {
            var startTime = 24;
            var endTime = 0;
            var movers = 0;
            for (var j = 0; j < data.length; j++) {
                for (var i = 0; i < data[j].Schedules.length; i++) {
                    if (data[j].Schedules[i].StartTime < startTime) {
                        startTime = data[j].Schedules[i].StartTime;
                    }
                    if (data[j].Schedules[i].EndTime > endTime) {
                        endTime = data[j].Schedules[i].EndTime;
                    }
                    movers += data[j].Schedules[i].Movers;

                }
                data[j].StartTime = startTime;
                data[j].EndTime = endTime;
                data[j].Movers = movers;
            }
           
            $scope.MovingToday = data;


        }, function(err) { alert("er"); });

    };
    $scope.GetStorageCount = function () {
        homeFactory.GetStorageCount().then(function (data) {

            $scope.StorageCount = data;

        }, function (err) { alert("er"); });

    };

    

    $scope.SetLeadCount = function() {
        
        homeFactory.GetLeadCount().then(function (data) {

            $scope.LeadCount = data;

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
    $scope.GetJobsForUser = function () {

        homeFactory.GetJobsForUser().then(function (data) {
            $scope.Jobs.Booked = data.Booked;
            $scope.Jobs.Posted = data.Posted;
            

        }, function (err) { alert("er"); });


    };
    
    $scope.Init = function() {
        

        $scope.GetRecentQuote();
        $scope.MovingToday();
        $scope.GetStats();
        $scope.SetLeadCount();
        $scope.GetStorageCount();
        $scope.GetJobsForUser();
        $scope.GetSurveys();
        $scope.GetMessages();
    };

    $scope.Init();

};
