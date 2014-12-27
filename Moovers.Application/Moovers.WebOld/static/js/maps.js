/// <reference path="references.js" />
/*!
    maps.js
*/

/*global google,Maps*/
window.Maps = {
    emptyOpts: {
        panControl: false,
        rotateControl: false,
        scaleControl: false,
        streetViewControl: false,
        overviewMapControl: false,
        mapTypeControl: false,
        zoomControl: false
    },
    drawMapAt: function(addr, container, cb) {
        var g = google.maps;
        var latlng = new g.LatLng(addr.Metadata.Latitude, addr.Metadata.Longitude);
        new g.MaxZoomService().getMaxZoomAtLatLng(latlng, function(res) {
            var opts = {
                center: latlng,
                zoom: res.zoom,
                mapTypeId: g.MapTypeId.SATELLITE
            };

            var fullOpts = $.extend(Maps.emptyOpts, opts);
            var map = new g.Map($(container)[0], fullOpts);
            if ($.isFunction(cb)) {
                cb(map);
            }
        });
    }
};