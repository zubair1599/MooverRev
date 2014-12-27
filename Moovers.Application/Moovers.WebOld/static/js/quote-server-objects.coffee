#/ <reference path="references.js" />
###!
    quote-server-objects.js
###
#global Utility,Inventory,Address,AddressTypes,StairTypes,ParkingTypes,ElevatorTypes,prettyDate
class this.PagedResult
    constructor: (resp) ->
        $.extend(this, resp);

class this.Quote 
    constructor: (resp) ->
        q = this;
        $.extend(this, resp);
        this.IsHourly = Utility.parseBool(resp.IsHourly);
        statuses = [ "Scheduled", "Open", "Deferred", "Lost" ];
        statuses.forEach( (i) -> q[i] = q.Status == i);

    active: () ->
        this.Status != "Won" && this.Status != "Lost";

class this.QuoteComment
    constructor: (resp) ->
        resp = resp ? {}
        { @Text, @UserName, @IsEditable } = resp;

        this.CommentID = resp.CommentID ? Utility.randomid();
            
        if (resp.Date and !resp.Date.getDate)
            this.Date = new Date(resp.Date);
        else
            this.Date = resp.Date;
            
    GetDateString: () ->
        if (new Date().toDateString() == this.Date.toDateString())
            return prettyDate(this.Date);
        else
            return this.Date.format("mmm d, yyyy' at 'h:MM TT");

    getCommentClass: () -> if (SERVER.username == this.UserName) then "logged-in-user" else ""
        
class this.Stop
    constructor: (obj) ->
        this.id = obj.id || Utility.randomid();
        $.extend(this, obj)

        this.walkDistance = parseInt(obj.walkDistance, 10);
        this.outsideStairsCount = parseInt(obj.outsideStairsCount, 10) || 0;
        this.insideStairsCount = parseInt(obj.insideStairsCount, 10) || 0;
        this.floor = parseInt(obj.floor, 10) || 0;
        this.apartmentComplex = obj.apartmentComplex || "";
        this.apartmentGateCode = obj.apartmentGateCode || "";
        this.liftgate = Utility.parseBool(obj.liftgate);
        this.dock = Utility.parseBool(obj.dock);
        this.storageDays = obj.storageDays || null;

        if (this.verified)
            this.verifiedAddress = obj.verifiedAddress;

        this.address = new Address(this.street1, this.street2, this.city, this.state, this.zip, this.addressid);

    addressTypeDisplay: () -> AddressTypes[this.addressType]

    parkingTypeDisplay: () -> ParkingTypes[this.parkingType]

    insideStairsTypeDisplay: () -> StairTypes[this.insideStairsType]

    outsideStairsTypeDisplay: () -> StairTypes[this.outsideStairsType]

    elevatorTypeDisplay: () -> ElevatorTypes[this.elevatorType]

    storageDaysDisplay: () -> 
        switch
            when (this.storageDays || 0) <= 0 then ""
            when (this.storageDays == 1) then "1 Day Storage"
            when (this.storageDays < 9999) then "#{@storageDays} Days Storage"
            else "Storage > 7 days"

class this.Item 
    constructor: (obj) ->
        $.extend(this, obj);
        this.AdditionalQuestions = _.map(obj.AdditionalQuestions, (i) -> new InventoryItemQuestion(i))

class InventoryItemQuestion
    constructor: (obj) ->
        $.extend(this, obj);

    getOptions: () ->
        _.chain(this.Options).sortBy( (i) -> i.Sort ).value();
        

class this.Room_Item_Rel
    constructor: (obj) ->
        $.extend(this, obj);

    renderTemplate: () -> Inventory.templates.itemTemplate(this);

    hasRelevantAdditionalInfo: () ->
        if (!this.AdditionalInfo)  then false;
        else _.any(this.AdditionalInfo, (i) -> i.DisplayName != "Frame");

    getAdditionalDescription: () ->
        _.chain(this.AdditionalInfo).filter( (i) ->
            i.DisplayName != "Frame" && i.DisplayName != "None"
        ).pluck("DisplayName").value().join();


class this.Room 
    constructor: (js) ->
        js = js || {};
        this.Type = js.Type || "";
        this.Description = js.Description || "";
        this.Items = _.map(js.Items || [], (i) -> new Room_Item_Rel(i) )
        this.StopID = js.StopID || ""
        this.StopName = js.StopName || ""
        this.RoomID = js.RoomID || Utility.randomid()
        this.Pack = js.Pack || false
        this.Boxes = js.Boxes || []
        this.Sort = js.Sort || 0
        this.IsUnassigned = js.IsUnassigned || false    

    addItem: (item) -> this.setItemCount(item, this.getQuantity(item) + 1);

    getItem: (id) ->
        rel = _(Inventory.currentRoom.Items).find( (i) -> (i.Item.Name == id || i.Item.ItemID == id));
        if (rel && rel.Item) then rel else undefined;

    getRoomClass: () -> if (!Inventory.editable) then "room no-edit" else "room";

    getItems: () -> _.chain(this.Items).filter( (i) -> i.Count > 0 ).value()

    setItemCount: (item, count, storageCount, additionalOptions) ->
        storageCount = storageCount or 0
        el = _.find(this.Items, (i) -> i.Item.ItemID is item.ItemID )
        if (el)
            if (el.Count > 0) then additionalOptions = el.AdditionalInfo;
            el.Count = count
            el.StorageCount = storageCount;
            el.IsSpecialItem = item.IsSpecialItem
        else
            sort = Math.max(_.max(_.pluck(this.Items, "Sort")), -1) + 1
            el = new Room_Item_Rel({
                Item: item
                Count: count
                StorageCount: storageCount
                Sort: sort
                IsBox: item.IsBox
                IsSpecialItem : item.IsSpecialItem
                Notes: ""
            })

            this.Items.push(el);

        el.AdditionalInfo = additionalOptions ? null;
        Inventory.setChanged(true);
        this.retemplate(item);

    renderBodyTemplate: (parentClass) ->
        bodyTemplate = Inventory.templates.roomBody
        item = $.extend({}, this, { divClass: parentClass || "room" })
        bodyContent = bodyTemplate(item)
        return bodyContent

    ## if "room" is unassigned, renders a summary of the quote
    ## if room is not unassigned, does nothing
    renderUnassigned: () ->
        if (!this.IsUnassigned) 
            return ""

        rooms = _.chain(Inventory.rooms).filter( (i) -> !i.IsUnassigned )
        return rooms.map( (i) => i.renderBodyTemplate("sub-room")).value().join("")

    moveItem: (toRoom, item) ->
        count = this.getQuantity(item) + toRoom.getQuantity(item)
        storage = this.getStorage(item) + toRoom.getStorage(item)
        additional = _.filter(this.Items, (i) -> i.Item.ItemID == item.ItemID).AdditionalInfo;
        this.setItemCount(item, 0);
        toRoom.setItemCount(item, count, storage, additional)

    retemplate: () ->
        menuTemplate = Inventory.templates.roomMenu
        menuContainer = Inventory.elements.roomMenu
        menuContent = $($.parseHTML($.trim(menuTemplate(this))))
        menuItem = menuContainer.find(".room-selector[data-roomid=" + this.RoomID + "]")
        if (menuItem.length > 0)
            menuItem.replaceWith(menuContent)
        else
            menuContainer.append(menuContent)

        bodyContent = $($.parseHTML(this.renderBodyTemplate()))
        bodyContainer = Inventory.elements.roomBody
        bodyItem = bodyContainer.find(".room[data-roomid=" + this.RoomID + "]")
        if (bodyItem.length > 0)
            bodyItem.replaceWith(bodyContent)
        else
            bodyContainer.append(bodyContent)

        if (this.isSelected())
            bodyContent.addClass("sel")
            menuContent.addClass("sel")

        Inventory.initDraggable(bodyContainer.find(".item[data-itemid]"))
        Inventory.initDroppable(menuContent)        
        if (!this.IsUnassigned)
            unassigned = Inventory.getUnassignedRoom()
            unassigned.retemplate()

    getQuantity: (item) ->
      resp = _.find(this.Items, (i) -> i.Item.ItemID is item.ItemID )
      if (resp) then resp.Count else 0

    getStorage: (item) ->
      resp = _.find(this.Items, (i) -> i.Item.ItemID is item.ItemID )
      if (resp) then resp.StorageCount else 0

    getItemCount: () ->
        items = this.Items
        if (this.IsUnassigned)
            items = _.chain(Inventory.rooms).pluck("Items").flatten().value()
        return _.filter(items, (i) -> i.Count > 0 ).length

    removeBox: (box) ->
        return if (!box) 
        this.Boxes = _.reject(this.Boxes, (i) -> i.BoxTypeID == box.BoxTypeID);

    setBoxCount: (box, count) ->
        this.removeBox(box)
        this.Boxes.push({
            BoxTypeID: box.BoxTypeID
            Count: count
            Name: box.Name
        })

    deleteItems: () -> 
        this.Items.empty();

    miniItemList: () ->
        ret = $.extend({}, this);
        ret.Items = ret.Items.map((i) -> 
            item = $.extend({}, i);
            item.Item = { ItemID: i.Item.ItemID }
            return item;
        )
        ret

    isSelected: () -> Inventory.currentRoom && (this.RoomID == Inventory.currentRoom.RoomID)