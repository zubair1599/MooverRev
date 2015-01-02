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


    $scope.ScheduleQuote = function () {


        $scope.model.rangestart = $scope.rangestart;
        $scope.model.rangeend = $scope.rangeend;
        $scope.model.movers = $scope.movers;
        $scope.model.paymentType = 'NO_CARD';
        $scope.model.quoteid = $scope.$parent.selectedQuote.QuoteID;


        scheduleFactory.ScheduleJob($scope.$parent.selectedQuote.QuoteID, $scope.model).then(function (data) {
            $scope.GetAllQuotesForDate();

        }, function (er) {

            alert("manageSchedule - scheduleFactory.ScheduleJob");
        });


    };


    $scope.GetAllQuotesForDate = function () {

        scheduleFactory.GetQuotesPerDay($scope.$parent.selectedQuote.Lookup, $scope.selectedDate, $scope.selectedMonth, $scope.selectedYear).then(function (quotes) {


            $scope.scheduleForQuote = quotes;

            $scope.scheduleDay = true;

            $scope.model.day = $scope.scheduleForQuote.Date;

        }
                , function (err) {


                    alert("manageSchedule - scheduleFactory.GetAllQuotes");
                });

    };

    $scope.OpenSchedule = function (index) {

        $scope.model.crew = index + 1;

    };

   


    $scope.getEventMonth = function (start, end, franchiseId) {

        scheduleFactory.GetScheduleForMonth(start, end, franchiseId).then(function (json) {

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
                        list.title = list.total + " Quotes";
                        $element.find('#calendar').fullCalendar('renderEvent', list, false);

                    }
                    if (survey.total > 0) {
                        survey.title = survey.total + " Surveys";


                        $element.find('#calendar').fullCalendar('renderEvent', survey, false);
                    }


                }
            }

        }, function (err) {

        });
    };


    $scope.GetSchedule = function () {

        scheduleFactory.GetScheduleForQuote($scope.$parent.selectedQuote.Lookup).then(function (data) {

            $scope.scheduleForQuote = data;


        }, function (err) {
            alert("manageSchedule - scheduleFactory.GetScheduleForQuote");
        });

    };

    $scope.InitCalendar = function() {
        


        var date = new Date();
        var d = date.getDate();
        var m = date.getMonth();
        var y = date.getFullYear();

        $element.find('#calendar').fullCalendar({
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            selectable: true,
            selectHelper: true,
            select: function (start, end, allDay) {

                var tmp = moment(start);
                var date = moment(start).date();
                var month = moment(start).month();
                var year = moment(start).year();
                month = month + 1;

                $scope.selectedDate = date;
                $scope.selectedMonth = month;
                $scope.selectedYear = year;

                $scope.GetAllQuotesForDate();


                $element.find('#calendar').fullCalendar('unselect');
            },
            editable: true,

            droppable: false, 
            drop: function (date, allDay) { 

                var originalEventObject = $(this).data('eventObject');

                var copiedEventObject = $.extend({}, originalEventObject);

                copiedEventObject.start = date;
                copiedEventObject.allDay = allDay;

                var labelClass = $(this).data('eventclass');

                if (labelClass) {
                    copiedEventObject.className = labelClass;
                }

                $('#calendar').fullCalendar('renderEvent', copiedEventObject, true);

                if ($('#drop-remove').is(':checked')) {
                    $(this).remove();
                }

            },


            events: function (start, end, timezone, callback) {
                var start1 = moment(start).format("MM/DD/YYYY");;
                var end1 = moment(end).format("MM/DD/YYYY");;
                $scope.getEventMonth(start1, end1, $scope.$parent.selectedQuote.FranchiseID);


            }
        });




    };


    $scope.Init = function () {

        $scope.InitCalendar();
        $scope.GetSchedule();



    };


    $scope.Init();

};
