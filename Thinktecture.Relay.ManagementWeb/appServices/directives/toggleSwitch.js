(function ($, jQuery) {
    "use strict";

    app.module.directive('toggleSwitch', function () {
        return {
            restrict: 'E',
            scope: {
                on: '=',
                onText: '@',
                offText: '@',
                toggle: '&'
            },
            templateUrl: 'appServices/directives/toggleSwitch.html'
        };
    });
})();
