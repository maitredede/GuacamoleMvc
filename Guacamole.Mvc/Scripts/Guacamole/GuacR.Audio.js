"use strict";

window.GuacR = window.GuacR || {};
/**
 * Object describing the UI's level of audio support. If the user has request
 * that audio be disabled, this object will pretend that audio is not
 * supported.
 */
window.GuacR.Audio = new (function () {

    var codecs = [
        'audio/ogg; codecs="vorbis"',
        'audio/mp4; codecs="mp4a.40.5"',
        'audio/mpeg; codecs="mp3"',
        'audio/webm; codecs="vorbis"',
        'audio/wav; codecs=1'
    ];

    var probably_supported = [];
    var maybe_supported = [];

    /**
     * Array of all supported audio mimetypes, ordered by liklihood of
     * working.
     */
    this.supported = [];

    // If sound disabled, we're done now.
    //if (GuacamoleSessionStorage.getItem("disable-sound", false))
    //    return;

    // Build array of supported audio formats
    codecs.forEach(function (mimetype) {
        try {
            var audio = new Audio();
            var support_level = audio.canPlayType(mimetype);

            // Trim semicolon and trailer
            var semicolon = mimetype.indexOf(";");
            if (semicolon != -1)
                mimetype = mimetype.substring(0, semicolon);

            // Partition by probably/maybe
            if (support_level == "probably")
                probably_supported.push(mimetype);
            else if (support_level == "maybe")
                maybe_supported.push(mimetype);
        }
        catch (err) {
            console.log("Error trying to test for codec support", mimetype, err);
        }
    });

    // Add probably supported types first
    Array.prototype.push.apply(
        this.supported, probably_supported);

    // Prioritize "maybe" supported types second
    Array.prototype.push.apply(
        this.supported, maybe_supported);

})();