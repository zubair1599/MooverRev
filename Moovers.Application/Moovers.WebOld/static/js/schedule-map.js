/// <reference path="references.js" />
/*global google, ScheduleMap */
/*!
    schedule-map.js
*/
(function(exports) {
    exports.ScheduleMap = {
        getIcon: function(crew, stop) {
            var crewColors = [
                "darkblue",
                "gray",
                "green",
                "lightblue",
                "limegreen",
                "orange",
                "purple",
                "red",
                "white",
                "yellow"
            ];

            return SERVER.baseUrl + "static/img/mapicons/" + crewColors[crew] + "/number_" + stop + ".png";
        },

        init: function(plots, franchiseAddress) {
            var franchiseLatLng = new google.maps.LatLng(franchiseAddress.Latitude, franchiseAddress.Longitude);
            var mapOptions = {
                zoom: 10,
                center: franchiseLatLng,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };

            var map = new google.maps.Map(document.getElementById("map"), mapOptions);
            var crews = _.groupBy(plots, function(i) {
                if (i.Crew.length <= 0) {
                    return "";
                }

                return i.Crew[0];
            });

            var added = [];
            var existingAddresses = [];
            _.each(crews, function(i, idx) {
                var crew = idx;
                if (!crew) {
                    return;
                }

                var tweakLatLong = function(addr) {
                    while (_.contains(existingAddresses, addr.Latitude + " " + addr.Longitude)) {
                        addr.Longitude += 0.02;
                    }
                };

                var count = 0;
                _.each(i, function(quote) {
                    count++;
                    tweakLatLong(quote.Origin);
                    existingAddresses.push(quote.Origin.Latitude + " " + quote.Origin.Longitude);
                    var originIcon = ScheduleMap.getIcon(crew - 1, count);

                    // add origin
                    var marker = new google.maps.Marker({
                        map: map,
                        position: new google.maps.LatLng(quote.Origin.Latitude, quote.Origin.Longitude),
                        title: "Origin " + quote.Lookup + "( Arrival Window: " + quote.ArrivalWindow + ")",
                        icon: originIcon
                    });
                    added.push(marker);

                    if (quote.Origin.Latitude === quote.Dest.Latitude && quote.Origin.Longitude === quote.Origin.Longitude) {
                        return;
                    }

                    count++;
                    tweakLatLong(quote.Dest);
                    existingAddresses.push(quote.Dest.Latitude + " " + quote.Dest.Longitude);

                    var destIcon = ScheduleMap.getIcon(crew - 1, count);
                    var markerDest = new google.maps.Marker({
                        map: map,
                        position: new google.maps.LatLng(quote.Dest.Latitude, quote.Dest.Longitude),
                        title: "Destination " + quote.Lookup + "( Arrival Window: " + quote.ArrivalWindow + ")",
                        icon: destIcon
                    });
                    added.push(markerDest);
                });
            });
        }
    };
})(window);


/***** NOTE :: This is no longer used, but may be useful again if we decide to draw labels again */

/*
 Google Maps Label implementation
function Label(opt_options) {
     Initialization
    this.setValues(opt_options);

     Label specific
    var span = this.span_ = document.createElement("span");
    span.classList.add("map-label");
    span.style.cssText = "position: relative; left: -50%; top: -8px; " +
                            "white-space: nowrap; border: 1px solid blue; " +
                            "padding: 2px; background-color: white; cursor: pointer!important;";

    var div = this.div_ = document.createElement("div");
    div.appendChild(span);
    div.style.cssText = "position: absolute; display: none";
}

Label.prototype = new google.maps.OverlayView();

 Implement onAdd
Label.prototype.onAdd = function() {
    var pane = this.getPanes().overlayLayer;
    pane.appendChild(this.div_);

     Ensures the label is redrawn if the text or position is changed.
    var me = this;
    this.listeners_ = [
        google.maps.event.addListener(this, "position_changed",
            function() { me.draw(); }),
        google.maps.event.addListener(this, "text_changed",
            function() { me.draw(); })
    ];
};

 Implement onRemove
Label.prototype.onRemove = function() {
    this.div_.parentNode.removeChild(this.div_);

     Label is removed from the map, stop updating its position/text.
    for (var i = 0, I = this.listeners_.length; i < I; ++i) {
        google.maps.event.removeListener(this.listeners_[i]);
    }
};

 Implement draw
Label.prototype.draw = function() {
    var projection = this.getProjection();
    var position = projection.fromLatLngToDivPixel(this.get("position"));

    var div = this.div_;
    div.style.left = position.x + "px";
    div.style.top = (position.y + 10) + "px";
    div.style.display = "block";

    this.span_.innerHTML = this.get("text").toString();
};*/