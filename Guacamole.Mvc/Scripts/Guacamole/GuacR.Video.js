"use strict";

window.GuacR = window.GuacR || {};
/**
 * Object describing the UI's level of video support.
 */
window.GuacR.Video = new (function () {

    var codecs = [
        'video/ogg; codecs="theora, vorbis"',
        'video/mp4; codecs="avc1.4D401E, mp4a.40.5"',
        'video/webm; codecs="vp8.0, vorbis"'
    ];

    var probably_supported = [];
    var maybe_supported = [];

    /**
     * Array of all supported video mimetypes, ordered by liklihood of
     * working.
     */
    this.supported = [];

    // Build array of supported audio formats
    codecs.forEach(function (mimetype) {

        var video = document.createElement("video");
        var support_level = video.canPlayType(mimetype);

        // Trim semicolon and trailer
        var semicolon = mimetype.indexOf(";");
        if (semicolon != -1)
            mimetype = mimetype.substring(0, semicolon);

        // Partition by probably/maybe
        if (support_level == "probably")
            probably_supported.push(mimetype);
        else if (support_level == "maybe")
            maybe_supported.push(mimetype);

    });

    // Add probably supported types first
    Array.prototype.push.apply(
        this.supported, probably_supported);

    // Prioritize "maybe" supported types second
    Array.prototype.push.apply(
        this.supported, maybe_supported);

})();
