'use strict';

var Lightbox = (function () {
    var Lightbox = function (url, width, height) {
        this.url = url;
        this.size = { width, height };
        this.normalizeUrl();
        this.crearModal();
    };

    Lightbox.prototype.normalizeUrl = function () {
        this.url = this.url.replace("watch?v=", "embed/");
    };

    Lightbox.prototype.crearModal = function () {
        var $backdrop = $("<div>", {
            className: "backdrop",
            css: {
                "position": "fixed",
                "top": "0",
                "left": "0",
                "width": "100%",
                "height": "100%",
                "background": "rgba(0,0,0,.7)",
                "z-index": "2000",
                "opacity": "0",
                "transition": "opacity .2s ease-in"
            },
            appendTo: window.document.body
        });
        var width = this.size.width;
        var height = this.size.height;
        var initialOverflow = $("body").css("overflow");
        var youtubeHTML = '<iframe width="' + width + '" height="' + height + '" src="' + this.url + '?rel=0&amp;showinfo=0&amp;autoplay=1" frameborder="0" allowfullscreen></iframe>';
        var $modal = $("<div>", {
            className: 'video_lightbox',
            html: youtubeHTML,
            css: {
                "position": "absolute",
                "top": (window.innerHeight - height) / 2,
                "left": (window.innerWidth - width) / 2,
                "width": width,
                "height": height,
                "background": "rgba(0,0,0,.7)",
            },
            appendTo: $backdrop
        });

        var resizeHook = function () {
            $modal.css({
                "top": (window.innerHeight - height) / 2,
                "left": (window.innerWidth - width) / 2,
            });
        };
        var hide = function (ev) {
            if (ev.target !== $backdrop.get(0)) return;
            $backdrop.css("opacity", 0);
            setTimeout(function () {
                $backdrop.remove();
                $("body").css("overflow", initialOverflow);
                $(window).unbind("resize", resizeHook)
            }, 200);
        };

        $("body").css("overflow", "hidden");
        $backdrop.bind("mousedown", hide);
        $(window).bind("resize", resizeHook);

        setTimeout(function () { $backdrop.css("opacity", 1) }, 10);

        this.$modal = $modal;
        this.$backdrop = $backdrop;
    }

    return {
        show: function (url, width, height) {
            return new Lightbox(url, width, height)
        }
    };
}());