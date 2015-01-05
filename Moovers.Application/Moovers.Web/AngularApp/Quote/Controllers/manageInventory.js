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
   
    $scope.AllRooms = [];

    $scope.AllInventories = [];



    $scope.ClearRoomBox = function() {

        $scope.selectedRoom = {};
        $scope.selectedRoom.Items = [];
        $scope.selectedRoom.Boxes = [];

    };


    $scope.SetAllRooms = function () {

        $scope.$parent.RefreshStops().then(function () {
        
            $scope.AllRooms = [];

            for (var i = 0; i < $scope.$parent.selectedQuote.Stops.length; i++) {

                $scope.AllRooms = $scope.AllRooms.concat($scope.$parent.selectedQuote.Stops[i].rooms);

            }
            $scope.InitInventory();
            $scope.GetUnassignedRoom();
        });
        

    };

    $scope.InitInventory = function () {
        $scope.AllInventories = [];
        for (var i = 0; i < $scope.AllRooms.length; i++) {

            var added = false;
            for (var j = 0; j < $scope.AllRooms[i].Items.length; j++) {

                var itemName = $scope.AllRooms[i].Items[j].Item.Name;
                var count = $scope.AllRooms[i].Items[j].Count;

                for (var k = 0; k < $scope.AllInventories.length; k++) {

                    if ($scope.AllInventories[k].Name === itemName) {
                        $scope.AllInventories[k].Count = $scope.AllInventories[k].Count + count;
                        added = true;
                        break;
                    }
                }
                if (!added) {
                    var item = new Object();
                    item.Name = itemName;
                    item.Count = count;
                    $scope.AllInventories.push(item);
                }
            }
        }

    };
   


    $scope.InventoryItems = function () {

        inventoryFactory.GetInventoryItems($scope.selectedQuote.Lookup).then(function () {
        }, function (e) { alert("Error in quote home - loading inventory"); });
    };


    $scope.onDrop = function($event, $data,room) {

        $scope.ChangeInventoryRoom($data, room);

    }


    $scope.ChangeInventoryRoom = function(item , room) {

        $scope.roomToDropItem = room;
        $scope.inventoryItemstoDrag = item;
        $scope.roomToDropItem.Items.push($scope.inventoryItemstoDrag);


        var newRoom = 0;
        for (var i = 0; i < $scope.AllRooms.length; i++) {
            if ($scope.AllRooms[i].RoomID == $scope.roomToDropItem.RoomID) {
                $scope.AllRooms[i] = $scope.roomToDropItem;
                newRoom = i;
            }
        }
        for (var j = 0; j < $scope.AllRooms.length; j++) {
           

            for (var k = 0; k < $scope.AllRooms[j].Items.length; k++) {
                
                    if ($scope.AllRooms[j].Items[k].RoomInverntoryItemID == item.RoomInverntoryItemID) {
                        if (j != newRoom) {
                        $scope.AllRooms[j].Items.splice(k, 1);
                        }
                    }
            }
        }

        var roomStr = JSON.stringify($scope.AllRooms);
        var quoteid = $scope.$parent.selectedQuote.QuoteID;
        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {

        }, function(e) {
            alert("Error");

        });

        
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


            for (var j = 0; j < $scope.AllRooms.length; j++) {

                if ($scope.AllRooms[j].RoomID == $scope.selectedRoom.RoomID) {
                    $scope.AllRooms[j] = $scope.selectedRoom;


                    var roomStr = JSON.stringify($scope.AllRooms);
                        var quoteid = $scope.$parent.selectedQuote.QuoteID;
                        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {
                            $scope.$parent.UpdateQuicklook();
                        }, function (e) {
                            $scope.$parent.RefreshStops();
                            alert("Error");

                        });


                       
                    }
                }
          
    };

    $scope.SetSelectedRoom = function(room) {
        $scope.selectedRoom = room;



    };

    $scope.UpdateRoom = function () {


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


        $scope.AllRooms.push($scope.selectedRoom);

        var roomStr = JSON.stringify($scope.AllRooms);
        var quoteid = $scope.$parent.selectedQuote.QuoteID;
        inventoryFactory.UpdateInventory(quoteid, roomStr).then(function(updatesJson) {
            $scope.SetAllRooms();
            $scope.Init();
        });


    };

   
  
    $scope.GetItemSuggestions = function() {

  

        
        $element.find('#searchtxt').autocomplete({
            source: inventoryFactory.searchItems,
            select: function (originalEvent,val) {
                for (var i = 0; i < inventoryFactory.searchItems.length; i++) {

                    if (val.item.value == inventoryFactory.InventoryList[i].Name) {
                        $scope.selectedItem = inventoryFactory.InventoryList[i];
                        $scope.searchQuery = val.item.value;
                        $scope.ItemAdditionalOptions();
                        $timeout(function() {
                            $scope.$apply();

                        });

                       
                        return false;
                        
                    }
                }

                return false;
            }
           
        });

       

    };



   $scope.GetUnassignedRoom = function () {


       var found = false;
          
            for (var i = 0; i < $scope.AllRooms.length; i++) {
                
                if ($scope.AllRooms[i].IsUnassigned=== true) {
                    found = true;

                    $scope.selectedRoom = $scope.AllRooms[i];
                    for (var j = 0; j < $scope.$parent.selectedQuote.Stops.length; j++) {

                        if ($scope.$parent.selectedQuote.Stops[j].id == $scope.AllRooms[i].StopID) {
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
                   $scope.AllRooms.push(unassigned);

               }
           
    };


    $scope.ItemAdditionalOptions = function() {


       

        if ($scope.selectedItem.AdditionalQuestions.length > 0) {
        $element.find('#options').empty();
        for (var i = 0; i < $scope.selectedItem.AdditionalQuestions.length; i++) {
            var questionId = 'divQuestion_' + i;
            var questionDiv = $('<div style="width:500px" id="' + questionId + '"> </div>');
            questionDiv.appendTo($element.find('#options'));
            var questionName = $('<label style="float:left;width:200px" for="' + i + '" >' + $scope.selectedItem.AdditionalQuestions[i].QuestionText + ': </label>');
            questionName.appendTo($element.find('#' + questionId));
            if ($scope.selectedItem.AdditionalQuestions[i].Options.length>0) {
                
                var list = $('<select class="form-control">').appendTo('#' + questionId);
                for (var j = 0; j < $scope.selectedItem.AdditionalQuestions[i].Options.length; j++) {
                    list.append($("<option>").attr('value', j).text($scope.selectedItem.AdditionalQuestions[i].Options[j].Option));

                }
            } else {
                var radioBtn = $('<input type="checkbox" name="check' + i + '" id="check' + i + '" />');
                radioBtn.appendTo($element.find('#' + questionId));
            }
            var newLine= $('<br/>');
            newLine.appendTo($element.find('#options'));
           
            }

                $window.OpenDialogT('additionalQuestions-modal');
            }
        



    };

    $scope.SaveItemOptions = function () {

       
        $window.CloseDialogT('additionalQuestions-modal');

        for (var i = 0; i < $scope.selectedItem.AdditionalQuestions.length; i++) {

            if ($scope.selectedItem.AdditionalQuestions[i].Options.length==0) {
                if ($scope.selectedItem.AdditionalQuestions[i].value == false || typeof $scope.selectedItem.AdditionalQuestions[i].value == 'undefined') {
                    $scope.selectedItem.AdditionalQuestions.splice(i, 1);
                }
            }

            
        }
    };


    $scope.ChangeOption = function (option) {


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

        for (var j = 0; j < $scope.AllRooms.length; j++) {

            if ($scope.AllRooms[j].RoomID == $scope.selectedRoom.RoomID) {
                $scope.AllRooms[j] = $scope.selectedRoom;


                var roomStr = JSON.stringify($scope.AllRooms);
                var quoteid = $scope.$parent.selectedQuote.QuoteID;
                inventoryFactory.UpdateInventory(quoteid, roomStr).then(function (updatesJson) {
                    $scope.$parent.UpdateQuicklook();
                   
                }, function (e) {
                   
                    alert("Error");

                });



            }
        }
        

    };





    $scope.Init = function () {

        $scope.SetAllRooms();
        $scope.ClearRoomBox();
       
        $scope.searchQuery = '';
        $scope.searchQuantity = '';
        $scope.searchStore = '';
        $scope.InventoryItems();
    };




    $scope.Init();
    
};