quoteApp.controller('manageInventory', ['inventoryFactory','$scope','$element','$window','$timeout', manageInventory]);

function manageInventory(inventoryFactory, $scope, $element, $window,$timeout) {

   
    $scope.Boxes = [];
    $scope.CustomBoxes = [];


    $scope.selectedRoom = {};
    $scope.selectedItem = {};
   
    $scope.selectedStop = {};
    $scope.tempOption = {};
    $scope.searchQuery = '';
    $scope.searchQuantity = 1;
    $scope.searchStore = 1;
   


    $scope.ClearRoomBox = function() {

        $scope.selectedRoom = {};
        $scope.selectedRoom.Items = [];
        $scope.selectedRoom.Boxes = [];

    };


    $scope.Init = function() {
       
        $scope.ClearRoomBox();
        $scope.GetUnassignedRoom();
        $scope.searchQuery = '';
        $scope.searchQuantity = '';
        $scope.searchStore = '';
    };




    $scope.onDrop = function($event, $data,room) {

        // alert($data.RoomID);
        $scope.ChangeInventoryRoom($data, room);

    }


    $scope.ChangeInventoryRoom = function(item , room) {

        $scope.roomToDropItem = room;
        $scope.inventoryItemstoDrag = item;
        //$scope.inventoryItemstoDrag.RoomInverntoryItemID = null;
        $scope.roomToDropItem.Items.push($scope.inventoryItemstoDrag);


        var newRoom = 0;
        for (var i = 0; i < $scope.$parent.AllRooms.length; i++) {
            if ($scope.$parent.AllRooms[i].RoomID == $scope.roomToDropItem.RoomID) {
                $scope.$parent.AllRooms[i] = $scope.roomToDropItem;
                newRoom = i;
            }
            //break;
        }
        for (var j = 0; j < $scope.$parent.AllRooms.length; j++) {
           

            for (var k = 0; k < $scope.$parent.AllRooms[j].Items.length; k++) {
                
                    if ($scope.$parent.AllRooms[j].Items[k].RoomInverntoryItemID == item.RoomInverntoryItemID) {
                        if (j != newRoom) {
                        $scope.$parent.AllRooms[j].Items.splice(k, 1);
                        }
                    }
            }
        }

        var roomStr = JSON.stringify($scope.$parent.AllRooms);
        var quoteid = $scope.$parent.selectedQuote.QuoteID;
        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {

            //$scope.$parent.RefreshStops();
            $scope.Init();
            //alert(updatesJson);
            //$scope.ClearRoomBox(); - undo comment to clear dialog
        }, function(e) {
            $scope.$parent.RefreshStops();
            alert("Error");

        });

        //alert();
        
    };
  
  

    $scope.AddItem = function() {

        if ($scope.searchQuantity == '') {
            $scope.searchQuantity = 1;
        }
        
        if ($scope.searchStore == '') {
            $scope.searchStore = 1;
        }
        

        var itemcol = new Object();
        itemcol.Count = $scope.searchQuantity;
        itemcol.StorageCount = $scope.searchStore;
        itemcol.Item = $scope.selectedItem;
        $scope.selectedRoom.Items.push(itemcol);


            for (var j = 0; j < $scope.$parent.AllRooms.length; j++) {

                if ($scope.$parent.AllRooms[j].RoomID == $scope.selectedRoom.RoomID) {
                    $scope.$parent.AllRooms[j] = $scope.selectedRoom;


                    var roomStr = JSON.stringify($scope.$parent.AllRooms);
                        var quoteid = $scope.$parent.selectedQuote.QuoteID;
                        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {

                            //$scope.$parent.RefreshStops();
                            $scope.Init();
                            //alert(updatesJson);
                            //$scope.ClearRoomBox(); - undo comment to clear dialog
                        }, function (e) {
                            $scope.$parent.RefreshStops();
                            alert("Error");

                        });


                       
                    }
                }
          
    };

    $scope.SetSelectedRoom = function(room) {
        //alert(room.Type);
        $scope.selectedRoom = room;



    };

    $scope.UpdateRoom = function () {


        //for edit rooms , check stop id  here
        if ($scope.selectedRoom.Type == 'Other') {
            $scope.selectedRoom.Type = $scope.selectedRoom.TypeOther;
        }

       $scope.selectedStop = {};

        for (var i = 0; i < $scope.$parent.selectedQuote.Stops.length; i++) {
            if ($scope.selectedRoom.StopID === $scope.$parent.selectedQuote.Stops[i].id) {

                $scope.selectedStop = $scope.$parent.selectedQuote.Stops[i];
                break;
            }
        }


        //fucntion takes json of all ROOMS
        $scope.$parent.AllRooms.push($scope.selectedRoom);

        //$scope.selectedStop.rooms.push($scope.selectedRoom);
        var roomStr = JSON.stringify($scope.$parent.AllRooms);
        var quoteid = $scope.$parent.selectedQuote.QuoteID;
        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function(updatesJson) {

            $scope.$parent.RefreshStops();
            $scope.Init();
            //alert(updatesJson);
            //$scope.ClearRoomBox(); - undo comment to clear dialog
        });


    };


  

    $scope.GetItemSuggestions = function() {

        $element.find('#searchtxt').autocomplete({
            lookup: inventoryFactory.searchItems,
            onSelect: function(val, originalEvent) {
                for (var i = 0; i < inventoryFactory.searchItems.length; i++) {
                    if (val == inventoryFactory.InventoryList[i].Name) {
                        $scope.selectedItem = inventoryFactory.InventoryList[i];
                        //$scope.ItemAdditionalOptions();
                        $timeout(function() {
                            $scope.$apply();

                        });

                        $('#additionalQuestions-modal').modal('show');
                        break;
                    }
                }

                return false;
            }
           
        });

       

    };



   $scope.GetUnassignedRoom = function () {


       var found = false;
          
       for (var i = 0; i < $scope.$parent.AllRooms.length; i++) {
                
           if ($scope.$parent.AllRooms[i].Sort == 9999) {
                found = true;

                $scope.selectedRoom = $scope.$parent.AllRooms[i];
                for (var j = 0; j < $scope.$parent.selectedQuote.Stops.length; j++) {

                    if ($scope.$parent.selectedQuote.Stops[j].id == $scope.$parent.AllRooms[i].StopID) {
                        $scope.selectedStop = $scope.$parent.selectedQuote.Stops[j];
                    }
                }
                    
                break;
            }
        }   
                
           


               if (!found) {
                   var unassigned = new Object();
                   unassigned.Type = "Unassigned";
                   unassigned.Description = "";
                   unassigned.StopID = $scope.$parent.selectedQuote.Stops[0].id;
                   unassigned.Sort = 9999;
                   unassigned.IsUnassigned = true;
                   unassigned.Items = [];
                   unassigned.Boxes = [];

                   $scope.selectedRoom = unassigned;
                   $scope.selectedStop = $scope.$parent.selectedQuote.Stops[0];
                   $scope.$parent.AllRooms.push(unassigned);

               }
           
        




    };


    $scope.ItemAdditionalOptions = function() {

        //$element.find('#options').empty();
        //for (var i = 0; i < $scope.selectedItem.AdditionalQuestions.length; i++) {
        //    var questionId = 'divQuestion_' + i;
        //    var questionDiv = $('<div style="width:500px" id="' + questionId + '"> </div>');
        //    questionDiv.appendTo($element.find('#options'));
        //    var questionName = $('<label style="float:left;width:200px" for="' + i + '" >' + $scope.selectedItem.AdditionalQuestions[i].QuestionText + ': </label>');
        //    questionName.appendTo($element.find('#' + questionId));
        //    if ($scope.selectedItem.AdditionalQuestions[i].Options.length>0) {
                
        //        //list 
        //        var list = $('<select>').appendTo('#' + questionId);
        //        for (var j = 0; j < $scope.selectedItem.AdditionalQuestions[i].Options.length; j++) {
        //            list.append($("<option>").attr('value', j).text($scope.selectedItem.AdditionalQuestions[i].Options[j].Option));

        //        }
        //    } else {
        //        //check box
        //        var radioBtn = $('<input type="checkbox" name="check' + i + '" id="check' + i + '" />');
        //        radioBtn.appendTo($element.find('#' + questionId));
        //    }
        //    var newLine= $('<br/>');
        //    newLine.appendTo($element.find('#options'));
        //}

        $('#additionalQuestions-modal').modal('show');


    };

    $scope.SaveItemOptions = function() {


        for (var i = 0; i < $scope.selectedItem.AdditionalQuestions.length; i++) {

            if ($scope.selectedItem.AdditionalQuestions[i].Options.length==0) {
                if ($scope.selectedItem.AdditionalQuestions[i].value == false || typeof $scope.selectedItem.AdditionalQuestions[i].value == 'undefined') {
                    $scope.selectedItem.AdditionalQuestions.splice(i, 1);
                }
            }

            
        }
    };


    $scope.ChangeOption = function (option) {
        //alert(option);

        //var a = $scope.chk0;

        //$('#additionalQuestions-modal').modal('hide');

        for (var i = 0; i < $scope.selectedItem.AdditionalQuestions.length; i++) {
            for (var j = 0; j < $scope.selectedItem.AdditionalQuestions[i].Options.length; j++) {
                if (option.OptionID === $scope.selectedItem.AdditionalQuestions[i].Options[j].OptionID) {

                    for (var k = 0; k < $scope.selectedItem.AdditionalQuestions[i].Options.length; k++) {
                        $scope.selectedItem.AdditionalQuestions[i].Options[k].Selected = false;
                    }
                   
                    $scope.selectedItem.AdditionalQuestions[i].Options[j].Selected = true;
                    break;
                }
            }
        }
        


    };


    $scope.RemoveItem = function(itemrel) {

        for (var i = 0; i < $scope.selectedRoom.Items.length; i++) {
            if ($scope.selectedRoom.Items[i].RoomInverntoryItemID ===itemrel.RoomInverntoryItemID) {
                $scope.selectedRoom.Items.splice(i, 1);
            }
        }

        for (var j = 0; j < $scope.$parent.AllRooms.length; j++) {

            if ($scope.$parent.AllRooms[j].RoomID == $scope.selectedRoom.RoomID) {
                $scope.$parent.AllRooms[j] = $scope.selectedRoom;


                var roomStr = JSON.stringify($scope.$parent.AllRooms);
                var quoteid = $scope.$parent.selectedQuote.QuoteID;
                inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {

                    //$scope.$parent.RefreshStops();
                    $scope.Init();
                    //alert(updatesJson);
                    //$scope.ClearRoomBox(); - undo comment to clear dialog
                }, function (e) {
                    $scope.$parent.RefreshStops();
                    alert("Error");

                });



            }
        }
        

    };
    $timeout(function () {
        $scope.Init();
       
    }, 3000);
    
};