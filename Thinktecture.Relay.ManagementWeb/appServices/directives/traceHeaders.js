(function ($, jQuery) {
    "use strict";

    app.module.directive('traceHeaders', function () {
        return {
            restrict: 'E',
            scope: {
                data: '='
            },
            templateUrl: 'appServices/directives/traceHeaders.html'
        }
    });
})();
