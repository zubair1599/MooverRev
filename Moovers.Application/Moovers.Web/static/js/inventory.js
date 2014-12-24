/// <reference path="references.js" />
/*!
    inventory.js
*/

/*global Quotes,Utility,Room,Keys,SearchFunctions*/
(function(exports) {
    var InventoryModal = {
        dialogs: {
            addCustom: "#custom-inventory-dialog"
        },
        show: function() {
            InventoryModal.dialogs.addCustom.modal("show");
        },
        hide: function() {
            InventoryModal.dialogs.addCustom.modal("hide");
        },
        init: function(onAdd) {
            Utility.initBase(InventoryModal);
            var modal = InventoryModal.dialogs.addCustom;
            function showError(prop, msg) {
                modal.find("[name=" + prop + "]").addClass("error");
                modal.find(".error").empty().append(msg).show();
            }

            modal.on("show", function() {
                modal.find(".error").empty().hide();
            });

            modal.find("form").bind("submit", function() {
                var data = $(this).serialize();
                var url = $(this).attr("action");
                var post = $.post(url, data);
                $(this).find(":input.error").removeClass("error");
                post.success(function(resp) {
                    if (resp.Errors) {
                        showError(resp.Errors[0].PropertyName, resp.Errors[0].ErrorMessage);
                        return;
                    }

                    InventoryModal.hide();
                    onAdd(resp);
                });
                post.error(function() {
                    showError("", "There was an error processing your request. Please try again");
                    return;
                });

                return false;
            });
        }
    };

    var Inventory = {
        currentRoom: null,
        hasChanged: false,
        isSliding: false,
        categories: [],
        options: [],
        selectedroominventoryitemid:null,
        setChanged: function(changed) {
            if (changed) {
                $("#inventory-price-loader").show();
            }

            Inventory.hasChanged = changed;
        },
        editable: true,
        dialogs: {
            additionalQuestions: "#inventory-item-question",
            packingList: "#packing-list",
            roomModal: "#add-room-modal",
            history: "#inventory-history"
        },
        templates: {
            roomMenu: "#room-menu-template",
            roomBody: "#room-body-template",
            itemTemplate: "#item-template",
            questionTemplate: "#inventory-item-question-template"
        },
        elements: {
            roomType: "select[name=roomType]",
            roomTypeCustom: "input[name=roomTypeCustom]",
            addRoom: "#add-room",
            addItemForm: "#add-item-form",
            addRoomButton: "#add-room-button",

            addItem: "#add-item",
            itemQuantity: "#add-item-quantity",
            itemStorage: "#add-item-storage",
            submit: "#add-item-submit",
            cancel: "#add-item-cancel",

            roomMenu: ".room-menu",
            roomBody: "#room-list"
        },
        urls: {
            saveInventory: SERVER.baseUrl + "Quote/Inventory"
        },
        getRoom: function(roomid) {
            return _.find(Inventory.rooms, function(room) {
                return room.RoomID === roomid;
            });
        },
        removeRoom: function(roomid) {
            Inventory.rooms = _.reject(Inventory.rooms, function(room) {
                return room.RoomID === roomid;
            });

            Inventory.elements.roomBody.find("[data-roomid=" + roomid + "]").remove();
            Inventory.elements.roomMenu.find("[data-roomid=" + roomid + "]").remove();
            Inventory.setChanged(true);

            // if there aren't any rooms, reload the entire page.
            if (!_.any(Inventory.rooms)) {
                Utility.showOverlay();

                (function poll() {
                    setTimeout(function() {
                        if (!$.ajaxq.isRunning() && !Inventory.hasChanged) {
                            window.location.reload();
                        }
                        else {
                            poll();
                        }
                    }, 100);
                })();
            }
        },

        // get an object without excess server values to store for versioning.
        getCondensedRooms: function() {
            return Inventory.rooms.map(function(r) {
                return {
                    RoomID: r.RoomID,
                    StopID: r.StopID,
                    Items: _.map(r.Items, function(i) {
                        return {
                            Count: i.Count,
                            Item: {
                                ItemID: i.Item.ItemID
                            }
                        };
                    })
                };
            });
        },

        getUnassignedRoom: function() {
            if (Inventory.stops.length === 0) {
                return null;
            }

            var unassigned = _.filter(Inventory.rooms, function(i) {
                return i.IsUnassigned;
            });

            if (unassigned.length > 0) {
                return unassigned[0];
            }

            // for old quotes, if they only have 1 room, make that the "unassigned" room.
            if (Inventory.rooms.length === 1) {
                unassigned = Inventory.rooms[0];
                unassigned.IsUnassigned = true;
                unassigned.Sort = 9999;
            }
            else {
                unassigned = new Room();
                unassigned.Type = "Unassigned";
                unassigned.Description = "";
                unassigned.StopID = _.first(Inventory.stops).id;
                unassigned.Sort = 9999;
                unassigned.IsUnassigned = true;
                Inventory.rooms.push(unassigned);
                unassigned.retemplate();
            }

            if (Inventory.editable) {
                Inventory.setChanged(true);
            }
            return unassigned;
        },

        selectRoom: function(roomid) {
            Inventory.elements.roomMenu.find(".room-selector").removeClass("sel");
            Inventory.elements.roomMenu.find(".room-selector[data-roomid=" + roomid + "]").addClass("sel");
            Inventory.elements.roomBody.find(".room").hide();
            Inventory.elements.roomBody.find(".room[data-roomid=" + roomid + "]").show();
            Inventory.currentRoom = Inventory.getRoom(roomid);
        },

        // init "inventory item" draggable
        initDraggable: function(items) {
            if (!Inventory.editable) {
                return;
            }

            // drag/drop is a pretty horrible experience on mobile
            if ($.browser.ipad || $.browser.iphone || $.browser.android) {
                return;
            }

            var settings = {
                revert: "invalid",
                revertDuration: 10
            };

            items.draggable(settings);
        },

        // init "inventory item" droppable
        initDroppable: function(items) {
            if (!Inventory.editable) {
                return;
            }

            var settings = {
                hoverClass: "dropper",
                accept: function(el) {
                    var dropped = $(this);
                    var fromRoomID = el.closest("div[data-roomid]").attr("data-roomid");
                    var roomid = dropped.data("roomid");
                    return fromRoomID !== roomid;
                },

                drop: function(event, ui) {
                    var dropped = $(this);
                    var el = ui.draggable;

                    var roomid = dropped.attr("data-roomid");
                    var itemid = el.data("itemid");
                    var roominverntoryitemid = el.data("roominverntoryitemid");
                    var item = Inventory.getItem(itemid);

                    var fromRoomID = el.closest("div[data-roomid]").attr("data-roomid");
                    var fromRoom = Inventory.getRoom(fromRoomID);
                    var toRoom = Inventory.getRoom(roomid);

                    fromRoom.moveItem(toRoom, item, roominverntoryitemid);
                    fromRoom.retemplate();
                    toRoom.retemplate();
                }
            };

            items.droppable(settings);
        },

        getFormRoom: function() {
            var roomid = $("#add-item-roomid").val();
            var room = roomid ? Inventory.getRoom(roomid) : Inventory.currentRoom;
            return room;
        },

        selectItem: function(id, room, roominventoryitemid) {
            var item = Inventory.getItem(id);
            if (!item) {
                item = room.getItem(id);
            }

            var existing = room.getQuantity(item, Inventory.options, roominventoryitemid);
            var storage = room.getStorage(item);

            $("#add-item-roomid").val(room.RoomID);
            Inventory.elements.addItem.val(item.Name);
            Inventory.elements.itemStorage.val(storage);
            Inventory.elements.itemQuantity.val(existing);
            Inventory.elements.itemQuantity.select();
            Inventory.elements.cancel.show();
            Inventory.elements.submit.text("Update Item");
        },
        showModal: function (item) {
            Inventory.options = [];
            var modal = Inventory.dialogs.additionalQuestions;
            var tmpl = Inventory.templates.questionTemplate;
            var content = tmpl(item);
            modal.find(".modal-container").empty().append(content);
            modal.modal("show");
        },
        getItem: function(itemName) {
            function findItem(i) {
                return i.RoomInverntoryItemID === itemName ||
                i.ItemID === itemName ||
                    $.trim((i.Name || "").toLowerCase()) === $.trim((itemName || "").toLowerCase());
            }

            var item = _.find(Inventory.inventoryItems, findItem);
            var custom = _.find(Inventory.customItems, findItem);
            return item || custom;
        },
        init: function(stops) {
            Inventory.editable = window.EDITABLE;

            window.onbeforeunload = function() {
                if (Inventory.hasChanged || $.ajaxq.isRunning()) {
                    return "There are changes pending, are you sure you'd like to leave this page?";
                }
            };

            Utility.initBase(Inventory);

            /*** Initialize with Server Data ***/
            Inventory.rooms = _.chain(stops).pluck("rooms").flatten().map(function(i) {
                return new Room(i);
            }).sortBy(function(i) {
                // order "unassigned" room to be the first sorted
                if (i.IsUnassigned) {
                    return -1;
                }
                return i.Sort;
            }).value();

            var unassigned = Inventory.getUnassignedRoom();

            _.each(Inventory.rooms, function(room) {
                room.retemplate();
            });

            if (Inventory.rooms.length > 0) {
                Inventory.selectRoom(unassigned.RoomID);
            }

            Inventory.elements.roomMenu.on("click", ".room-selector", function() {
                var roomId = $(this).data("roomid");
                Inventory.selectRoom(roomId);
            });

            if (!Inventory.editable) {
                return;
            }

            InventoryModal.init(function(item) {
                Inventory.customItems.push(item);
                Inventory.elements.addItem.text(item.Name);
                Inventory.elements.itemQuantity.focus();
            });

            $("#allToStorage").click(function() {
                var room = Inventory.currentRoom;

                _.each(Inventory.rooms, function(r) {
                    _.each(r.Items, function(i) {
                        r.setItemCount(i.Item, i.Count, i.Count, i.AdditionalInfo);
                    });
                });

                Inventory.selectRoom(room.RoomID);
            });

            $("#allFromStorage").click(function() {
                var room = Inventory.currentRoom;
                _.each(Inventory.rooms, function(r) {
                    _.each(r.Items, function(i) {
                        r.setItemCount(i.Item, i.Count, 0, i.AdditionalInfo);
                    });
                });

                Inventory.selectRoom(room.RoomID);
            });

            Inventory.elements.roomMenu.on("click", ".room-selector .icon-edit", function() {
                var roomid = $(this).closest(".room-selector").data("roomid");
                var room = Inventory.getRoom(roomid);
                var dialog = Inventory.dialogs.roomModal;
                var roomType = dialog.find("[name=roomType]");
                var roomTypeCustom = dialog.find("[name=roomTypeCustom]").val(room.Type);
                dialog.find("[name=roomDescription]").val(room.Description);

                dialog.find("[name=roomid]").val(room.RoomID);

                roomType.find("option").removeAttr("selected");

                var option = roomType.find("option").filter(function() {
                    return $(this).html() === room.Type;
                });

                if (option.length > 0) {
                    option.prop("selected", true);
                    roomTypeCustom.hide();
                }
                else {
                    roomType.find("option").filter(function() {
                        return $(this).val() === "Other";
                    }).prop("selected", true);

                    roomTypeCustom.show();
                }

                dialog.modal("show");
                return false;
            });


            var data = _.map(Inventory.inventoryItems, function(c) {
                return {
                    value: c.Name,
                    alts: _.flatten([[(c.KeyCode || "").toString()], c.Aliases])
                };
            });

            Inventory.elements.roomTypeCustom.hide();

            /*** Setup Interactive Elements *****/
            Inventory.elements.addItem.blur(function() {
                var me = this;
                var isWindow = false;
                Inventory.options = [];
                // There was an issue here where the "AddCustom" modal would be shown on window blur, even when an item was selected
                // This allows the autocomplete box to function correctly in this relatively uncommon case.
                $(window).bind("blur.crappy-workaround", function() {
                    isWindow = true;
                });

                setTimeout(function() {
                    $(window).unbind("blur.crappy-workaround");

                    if (isWindow) {
                        return;
                    }

                    var txt = $(me);
                    var item = Inventory.getItem(txt.val());
                    var val = $.trim(txt.val());

                    if (!val) {
                        return;
                    }

                    if (_.isUndefined(item)) {
                        InventoryModal.show();
                        InventoryModal.dialogs.addCustom.find("[name=name]").val(txt.val());
                    }
                     //if (Inventory.currentRoom.getQuantity(item) === 0 && item.AdditionalQuestions.length > 0) {// removed the first condition for adding addendmums all time
                    if (item.AdditionalQuestions.length > 0 && (Inventory.selectedroominventoryitemid == null || !Inventory.selectedroominventoryitemid)) {
                        Inventory.showModal(item);
                    }
                }, 10);
            });

            Inventory.dialogs.additionalQuestions.bind("keypress", function(e) {
                if (e.keyCode === Keys.ENTER || e.keyCode === Keys.NUMPAD_ENTER) {
                    $("#add-item-with-questions").click();
                }
            });

            $("#add-item-with-questions").click(function() {
                var formContainer = $("#inventory-item-question .modal-container");
                var options = [];
                formContainer.find(":input[name]").each(function() {
                    var input = $(this);
                    var questionid = input.attr("name");
                    if (input.attr("type") === "checkbox" && input.is(":checked")) {
                        options.push({ QuestionID: questionid, OptionID: null, DisplayName: input.data("displayname") });
                    }
                    else if (input.is("select")) {
                        var val = input.val();
                        var selected = input.find("option[value=" + val + "]");
                        options.push({ QuestionID: questionid, OptionID: val, DisplayName: selected.data("displayname") });
                    }
                });
                Inventory.options = options;
                var itemid = formContainer.find("[name=itemid]").val();
                var item = Inventory.getItem(itemid);
                var count = Math.max(Inventory.currentRoom.getQuantity(item, options), 1);

                var room = Inventory.getFormRoom();
                var storage = room.getStorage(item);

                room.setItemCount(item, count, storage, options);
                Inventory.selectItem(itemid, room);

                $(".modal.in").modal("hide");
                Inventory.elements.itemQuantity.focus().select();
                
                return false;
            });

            Inventory.elements.addItem.autocomplete({
                lookup: data,
                partialMatch: true,
                selectFirst: true,
                maxSuggestions: 10,
                orderBy: SearchFunctions.itemAutocompleteSort,
                onSelect: function(val, originalEvent) {
                    Inventory.selectItem(Inventory.elements.addItem.val(), Inventory.getFormRoom());
                    if (originalEvent.keyCode === Keys.TAB) {
                        originalEvent.preventDefault();
                    }

                    return false;
                }
            });

            //setInterval(function() {
            //    var last = StateManager.getLatest("quote-inventory", Quotes.lookup);
            //    StateManager.store("quote-inventory", Quotes.lookup, JSON.stringify(Inventory.getCondensedRooms()));
            //}, 1000);

            Quotes.bindSave(function(success, error, always) {
                if (Inventory.hasChanged && !$.ajaxq.isRunning()) {
                    var quoteid = Quotes.quoteid;
                    var rooms = JSON.stringify(Inventory.rooms);

                    $("#inventory-loading").show();

                    var ajax = $.postq("quotes", Inventory.urls.saveInventory, { quoteid: quoteid, rooms: rooms }, function(resp) {
                        Quotes.updateQuicklook();

                        // update Guids with server response.
                        _.each(resp, function(val, key) {
                            var room = Inventory.getRoom(key);
                            var roomcontainer = $("[data-roomid=" + key + "]").attr("data-roomid", val);
                            roomcontainer.find("[name=roomid]").val(val);
                            roomcontainer.find(".remove").show();
                            if (room) {
                                room.RoomID = val;
                            }
                        });
                    });

                    ajax.always(function() {
                        $("#inventory-loading").hide();
                    });

                    Inventory.setChanged(false);
                    ajax.success(success);
                    ajax.error(error);
                    ajax.always(always);
                }
            });

            /**** Event Binding ****/

            function resetAddItem() {
                Inventory.elements.addItemForm[0].reset();
                $("#add-item-roomid").val("");
                Inventory.elements.cancel.hide();
                Inventory.elements.submit.text("Add Item");
            }

            Inventory.elements.cancel.click(function() {
                resetAddItem();
                Inventory.selectedroominventoryitemid = null;
                Inventory.options = [];
                return false;
            });

            Inventory.elements.itemQuantity.click(function() {
                $(this).select();
            });

            Inventory.elements.itemStorage.click(function() {
                $(this).select();
            });

            Inventory.elements.addItemForm.submit(function() {
                var val = Inventory.elements.addItem.val();
                var item = Inventory.getItem(val) || Inventory.currentRoom.getItem(val);
                var count = parseInt(Inventory.elements.itemQuantity.val(), 10) || 0;
                var storage = Math.min(parseInt(Inventory.elements.itemStorage.val(), 10) || 0, count);
                if (!item) {
                    return false;
                }

                var room = Inventory.getFormRoom();
                room.setItemCount(item, count, storage, Inventory.options, Inventory.selectedroominventoryitemid);
                this.reset();
                Inventory.selectedroominventoryitemid = null;
                Inventory.options = [];

                $("#add-item").focus();

                resetAddItem();
                return false;
            });

            // for "Other" rooms, show a box and make the name of the room required
            Inventory.elements.roomType.change(function() {
                var selected = ($(this).val() === "Other");
                Inventory.elements.roomTypeCustom.toggle(selected);
                Inventory.elements.roomTypeCustom.toggleAttr("required", selected);
            });

            Inventory.elements.roomBody.on("click", ".item[data-itemid]", function() {
                if (Inventory.editable) {
                    var itemid = $(this).data("itemid");
                    var roominventoryitemid = $(this).data("roominverntoryitemid");
                    var roomid = $(this).closest("div[data-roomid]").attr("data-roomid");
                    var room = Inventory.getRoom(roomid);
                    Inventory.selectedroominventoryitemid = roominventoryitemid;
                    Inventory.selectItem(itemid, room, roominventoryitemid);
                }
            });

            Inventory.elements.roomBody.on("click", ".remove-item", function() {
                var itemid = $(this).closest("[data-itemid]").data("itemid");
                var roomid = $(this).closest("div[data-roomid]").attr("data-roomid");
                var roominventoryitemid = $(this).data("roominverntoryitemid");
                var room = Inventory.getRoom(roomid);
                var item = Inventory.getItem(itemid);
                room.setItemCount(item, 0, 0, null, roominventoryitemid);
                resetAddItem();
                return false;
            });

            Inventory.elements.roomMenu.on("click", "span.remove", function() {
                var roomid = $(this).closest("[data-roomid]").attr("data-roomid");
                var room = Inventory.getRoom(roomid);

                var msg = "Are you sure you want to remove the room: " + room.Type;
                if (room.Description) {
                    msg += ", " + room.Description;
                }

                if (!window.confirm(msg)) {
                    return false;
                }

                var unassigned = Inventory.getUnassignedRoom();
                _.each(room.Items, function(i) {
                    room.moveItem(unassigned, i.Item, i.RoomInverntoryItemID);
                });

                if (Inventory.currentRoom.RoomID === room.RoomID) {
                    Inventory.selectRoom(unassigned.RoomID);
                    
                }

                Inventory.removeRoom(room.RoomID);
                unassigned.retemplate();
                return false;
            });

            Inventory.elements.addRoomButton.click(function() {
                Inventory.dialogs.roomModal.modal("show");
            });

            Inventory.elements.addRoom.on("keyup", "select", function(e) {
                if (e.keyCode === Keys.ENTER || e.keyCode === Keys.NUMPAD_ENTER) {
                    $(this).closest("form").find("[type=submit]").click();
                }
            });

            Inventory.dialogs.roomModal.bind("hide", function() {
                Inventory.elements.addRoom[0].reset();
                Inventory.elements.addRoom.find("[name=roomid]").val("");
                Inventory.elements.addRoom.find("[name=roomTypeCustom]").hide();
            });

            Inventory.dialogs.roomModal.bind("shown", function() {
                $(this).find("[name=roomType]").focus();
            });

            var addState = function() {
                var obj = {
                    quoteid: Quotes.quoteid,
                    time: new Date(),
                    inventory: JSON.stringify(Inventory.rooms.map(function(i) { return i.miniItemList(); }))
                };

                StateManager.store("quote-inventory", obj, 15);
            };

            var restoreState = function(state) {
                var rooms = JSON.parse(state);
                _.each(rooms, function(r) {
                    var room = Inventory.getRoom(r.RoomID);
                    if (room) {
                        room.deleteItems();
                        room.retemplate();
                    }
                });                

                _.each(rooms, function(r) {
                    var room = Inventory.getRoom(r.RoomID);
                    var items = r.Items;
                    if (!room) {
                        room = new Room(r);
                        room.RoomID = Utility.randomid();
                        room.Items.empty();
                        Inventory.rooms.push(room);
                    }
                    _.each(items, function(i) {
                        room.setItemCount(Inventory.getItem(i.Item.ItemID), i.Count, i.StorageCount, i.AdditionalInfo);
                    });

                    room.retemplate();
                    Inventory.hasChanged = true;
                });

                Inventory.dialogs.history.modal("hide");                
            };

            $("#inventory-history").on("click", "[type=submit]", function() {
                var selected = $(".history-container").find("input:checked").val();
                restoreState(selected);
            });

            $("#removeAll").click(function() {
                if (confirm("Are you sure you want to delete ALL items?")) {
                    Utility.showOverlay();
                    _.each(Inventory.rooms, function(r) {
                        Inventory.rooms.empty();
                    });

                    Inventory.hasChanged = true;
                    setInterval(function() {
                        if (!$.ajaxq.isRunning()) {
                            window.location.reload();
                        }
                    }, 1000);
                }
            });

            $("#undoRepo").click(function() {
                var state = StateManager.getAll("quote-inventory");
                state = _(state).filter(function(i) { return i.quoteid === Quotes.quoteid; }).map(function(i) {
                    var obj = i;
                    i.inventory = JSON.parse(i.inventory);
                    i.time = new Date(i.time);
                    i.getItemCount = function() {
                        return _.sum(i.inventory.map(function(x) {
                            return _.sum(x.Items, function(y) {
                                return y.Count;
                            });
                        }));                    
                    };

                    return i;
                });

                var modal = Inventory.dialogs.history.modal("show");
                var content = $("#history-template").html();
                var tmpl = _.template(content);
                var items = state.map(function(i) {
                    return tmpl(i);
                });

                modal.find(".history-container").empty().append(items);
            });

            Inventory.elements.addRoom.add(Inventory.elements.addItemForm).submit(function() {
                addState();
            });

            Inventory.elements.addRoom.submit(function() {
                var form = $(this);
                var obj = form.serializeObject();

                if (obj.roomType === "Other") {
                    obj.roomType = Inventory.elements.roomTypeCustom.val();
                }
                if (!obj.roomType) {
                    return false;
                }

                var roomid = $.trim(form.find("[name=roomid]").val());
                var room;
                if (roomid) {
                    room = Inventory.getRoom(roomid);
                }
                else {
                    room = new Room();
                    room.Sort = Math.max(_.chain(Inventory.rooms).pluck("Sort").filter(function(i) {
                        return i !== 9999;
                    }).max().value(), 0) + 1;

                    Inventory.rooms.push(room);
                }

                room.Type = obj.roomType;
                room.Description = obj.roomDescription;
                room.StopName = form.find("[name=roomStop] option[value='" + obj.roomStop + "']").text();
                room.StopID = obj.roomStop;
                room.retemplate();

                Inventory.selectRoom(room.RoomID);
                Inventory.setChanged(true);

                form[0].reset();
                Inventory.dialogs.roomModal.modal("hide");
                Inventory.elements.roomTypeCustom.hide().removeAttr("required");
                return false;
            });
        }
    };

    exports.Inventory = Inventory;
})(window);