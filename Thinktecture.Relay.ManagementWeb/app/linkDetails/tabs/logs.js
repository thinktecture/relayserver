!function ($, jQuery, window, document) {
    'use strict';

    app.module.directive('linkLogsTab',
        /**
         * @param $translate
         * @param {Log} log
         * @param {AppEvents} appEvents
         * @param {SpinnerOverlay} spinnerOverlay
         */
        function ($translate, log, appEvents, spinnerOverlay) {
            return {
                restrict: 'E',
                templateUrl: 'app/linkDetails/tabs/logs.html',
                link: function (scope) {
                    function reloadLogs() {
                        scope.areLogsReloading = true;
                        spinnerOverlay.open();

                        log.getLatestLogs(scope.link)
                            .then(function (data) {
                                scope.logs = data;
                            }, function () {
                                $translate('LINK_DETAILS.NOTIFICATIONS.LOG_ERROR')
                                    .then(function (text) {
                                        notificationService.error(text);
                                    });
                            })
                            .finally(function () {
                                scope.areLogsReloading = false;
                                spinnerOverlay.close();
                            });
                    }

                    scope.$on(appEvents.reloadLogs, reloadLogs);

                    scope.reloadLogs = reloadLogs;

                    // Reload logs, if scope.link changes and the current active tab is logs
                    // Happens, when the user reloads the app while viewing the logs
                    // "TabActivated" Broadcast will be executed before scope.$on in this directive is executed
                    var unwatch = scope.$watch('link', function (newVal) {
                        if (newVal && scope.activeTabIndex === 2) {
                            unwatch();
                            reloadLogs();
                        }
                    });
                }
            };
        });
}();
