(function () {
    "use strict";

    function getQueryValue(val) {
        var result = "",
            tmp = [];
        location.search
            //.replace ( "?", "" )
            // this is better, there might be a question mark inside
            .substr(1)
            .split("&")
            .forEach(function (item) {
                tmp = item.split("=");
                if (tmp[0] === val) result = decodeURIComponent(tmp[1]);
            });
        return result;
    }

    var apiHost = '/';

    var queryValue = getQueryValue('apiHost');

    if (queryValue !== "") {
        apiHost = queryValue;
    }

    app.module.constant('apiHost', apiHost);
    app.module.constant('apiUrl', apiHost + 'api/managementweb/');
    app.module.constant('apiTimeout', 30000); // milliseconds

    app.module.config(function ($translateProvider, translationsEn) {
        $translateProvider.useSanitizeValueStrategy('sanitize');
        $translateProvider
            .translations('en', translationsEn)
            .preferredLanguage('en');
    });

    /**
     * @public
     * @constructor
     */
    function AppEvents() {
        this.reloadChart = 'reloadChart';
        this.reloadLogs = 'reloadLogs';
        this.reloadTraces = 'reloadTraces';
    }

    app.module.constant('appEvents', new AppEvents());
})();
