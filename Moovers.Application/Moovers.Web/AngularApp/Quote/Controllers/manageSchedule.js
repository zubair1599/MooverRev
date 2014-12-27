quoteApp.controller('manageSchedule', ['scheduleFactory', '$scope', '$element', '$window', '$timeout', manageSchedule]);

function manageSchedule(scheduleFactory, $scope, $element, $window, $timeout) {


    $scope.selectedDate = null;
    $scope.selectedMonth = null;
    $scope.selectedYear = null;
    $scope.scheduleForQuote = null;
    $scope.scheduleDay = false;


    $scope.model = new Object();
    $scope.model.rangestart = '';
    $scope.model.rangeend = '';
    $scope.model.movers = '';
    $scope.model.HasCard = false;

    $scope.rangestart = 5;
    $scope.rangeend = 5;
    $scope.movers = 2;


    $scope.ScheduleQuote = function() {


        //var model = new Object();
        $scope.model.rangestart = $scope.rangestart;
        $scope.model.rangeend = $scope.rangeend;
        $scope.model.movers = $scope.movers;
        $scope.model.paymentType = 'NO_CARD';
        $scope.model.quoteid = $scope.$parent.selectedQuote.QuoteID;
        //$scope.model.crew = crew;


        scheduleFactory.ScheduleJob($scope.$parent.selectedQuote.QuoteID, $scope.model).then(function(data) {
                

            }, function (er) {

                alert("manageSchedule - scheduleFactory.ScheduleJob");
        });


    };


    $scope.GetAllQuotesForDate = function() {

        scheduleFactory.GetQuotesPerDay($scope.$parent.selectedQuote.Lookup,$scope.selectedDate, $scope.selectedMonth, $scope.selectedYear).then(function(quotes) {


            $scope.scheduleForQuote = quotes;
            
            
            $scope.model.day = $scope.scheduleForQuote.Date;

            }
                , function(err) {
            
                
                alert("manageSchedule - scheduleFactory.GetAllQuotes");
        });

    };

    $scope.OpenSchedule = function(index) {
        
        //$scope.scheduleForQuote.CrewSize[index]
        $scope.model.crew = index+1;

    };

    $scope.GetSchedule = function() {

        scheduleFactory.GetScheduleForQuote($scope.$parent.selectedQuote.Lookup).then(function(data) {

            $scope.scheduleForQuote = data;


        }, function(err) {
            alert("manageSchedule - scheduleFactory.GetScheduleForQuote");
        });

    };





    $scope.Init = function() {
        

        $scope.GetSchedule();
        $element.find('#calendar').fullCalendar({


            eventLimit: true, // allow "more" link when too many events
            //events: $scope.Array,
            cache: false,
            dayClick: function (date, jsEvent, view) {

                $scope.selectedDate = date.date();
                $scope.selectedMonth = date.month();
                $scope.selectedYear = date.year();
                $scope.GetAllQuotesForDate();

            },
            eventSources: {

                url: '/Quote/GetSchedule1',
                type: 'GET',
                data: {
                    //start1: '123',
                    //end1: 'end',
                    franchiseid: 'da5605a7-ce5c-4253-934e-7f7fa72ce12d'//$scope.$parent.franchiseID
                },
                success: function (json) {

                    // $element.find('#calendar').fullCalendar('removeEvents', event.id);
                    $element.find('.fc-event').remove();
                    var done = [];
                    for (var i = 0; i < json.length; i++) {
                        var list = {};
                        var survey = {};

                        var otherEvent = {};

                        list = {};
                        list.total = 0;
                        survey = {};
                        survey.total = 0;

                        var tmpdate = moment.unix(json[i].start).date();
                        var tmpmonth = moment.unix(json[i].start).month();

                        var a = tmpdate + " " + tmpmonth;

                        if (done.indexOf(a) == -1) {

                            done.push(a);
                            for (var j = 0; j < json.length; j++) {
                                if (moment.unix(json[j].start).date() == tmpdate && moment.unix(json[j].start).month() == tmpmonth) {


                                    if (json[j].title.indexOf("Quote") > -1) {
                                        list.total = list.total + 1;
                                        list.start = moment.unix(json[j].start);
                                    } else if (json[j].title.indexOf("Home") > -1) {
                                        survey.total = survey.total + 1;
                                        survey.start = moment.unix(json[j].start);
                                    } else {
                                        otherEvent.start = moment.unix(json[j].start);
                                        otherEvent.title = json[j].title;
                                        $element.find('#calendar').fullCalendar('renderEvent', otherEvent, false);
                                    }



                                }

                            }
                            if (list.total > 0) {
                                list.title = list.total + " Quotes...";
                                $element.find('#calendar').fullCalendar('renderEvent', list, false);

                            }
                            if (survey.total > 0) {
                                survey.title = survey.total + " Surveys";


                                $element.find('#calendar').fullCalendar('renderEvent', survey, false);
                                //$('#calendar').fullCalendar('removeEvents', event.id);
                            }
                            //$('#calendar').fullCalendar('removeEvents', event.id);


                        }









                    }
                    //$('#calendar').fullCalendar('removeEvents', event.id);
                    //alert(data);


                },
                error: function () {
                    alert('there was an error while fetching events!');
                },
                //color: 'yellow',   // a non-ajax option
                //textColor: 'black' // a non-ajax option
            }
        });




    };


    $scope.Init();

};
