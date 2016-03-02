(function () {
    "use strict";

    window.app = window.app || { resolver: {} };

    app.module = angular.module('thinktectureRelayAdminWeb', [
        'ui.router',
        'ui.bootstrap',
        'pascalprecht.translate',
        'ui.notify',
        'ui.grid',
        'chart.js',
        'ngSanitize'
    ]);
})();
