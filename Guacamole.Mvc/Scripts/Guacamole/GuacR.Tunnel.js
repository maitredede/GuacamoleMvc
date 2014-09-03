"use strict";

window.GuacR = window.GuacR || {};

//Tunnel
window.GuacR.SignalRTunnel = function () {
    var that = this;
    var hub = $.connection.guacHub;

    $.connection.hub.error(function (error) {
        console.log('SignalR error: ' + error)
    });
    //$.connection.hub.logging = true;

    hub.client.Receive = function (elements) {
        // Get opcode
        var opcode = elements.shift();
        // Call instruction handler.
        if (that.oninstruction)
            that.oninstruction(opcode, elements);
    };

    this.connect = function (data) {
        var dfd = new $.Deferred();

        that.changeState(Guacamole.Tunnel.State.CONNECTING);

        $.connection.hub.start().done(function () {
            hub.server.connect(data.id, data.audio, data.video).done(function () {
                that.changeState(Guacamole.Tunnel.State.OPEN);
                dfd.resolve();
            }).fail(function () {
                that.changeState(Guacamole.Tunnel.State.CLOSED);
                dfd.reject();
            });
        }).fail(function () {
            that.changeState(Guacamole.Tunnel.State.CLOSED);
            dfd.reject();
        })
        return dfd;
    }
    this.disconnect = function () {
        var dfd = new $.Deferred();

        that.changeState(Guacamole.Tunnel.State.CLOSED);

        try{
            $.connection.hub.stop();
            dfd.resolve();
        }
        catch(err)
        {
            if(!dfd.isResolved)
                dfd.reject();
        }
        return dfd;
    }
    this.sendMessage = function () {
        var arr = [];
        for (var i = 0; i < arguments.length; i++) {
            arr.push(arguments[i]);
        }
        hub.server.send(arr).fail(function (e) {
            console.log("sendMessage error", e);
            that.changeState(Guacamole.Tunnel.State.CLOSED);
        });
    }

    this.changeState = function (newState) {
        that.state = newState;
        if (that.onstatechange)
            that.onstatechange(that.state);
    }
}
window.GuacR.SignalRTunnel.prototype = new Guacamole.HTTPTunnel();