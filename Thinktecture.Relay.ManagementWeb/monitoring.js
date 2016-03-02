var nconf = require('nconf'),
    http = require('http'),
    express = require('express');

nconf.file('monitoring.json');

if (!nconf.get('relayServer')) {
    console.error('Relay Server not configured.');
    return;
}

if (!nconf.get('link')) {
    console.error('Link not configured.');
    return;
}

if (!nconf.get('password')) {
    console.error('Password not configured.');
    return;
}

if (!nconf.get('port')) {
    console.error('Port not configured.');
    return;
}

function httpCallback(res) {
    var result;

    res.on('data', function (chunk) {
        result += chunk;
    });

    res.on('end', function () {
        httpResult(result);
    })
}

function httpResult(body) {
    console.log(body);
}

function sendMonitoringPing() {
    var options = {
        host: nconf.get('relayServer:host'),
        port: nconf.get('relayServer:port'),
        path: '/relay/' + nconf.get('link') + '/ping'
    };

    http.get(options, httpCallback);
}

sendMonitoringPing();