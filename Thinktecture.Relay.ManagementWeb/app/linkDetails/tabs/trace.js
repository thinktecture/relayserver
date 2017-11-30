!function ($, jQuery, window, document) {
    'use strict';

    app.module.directive('linkTraceTab',
        /**
         * @param $interval
         * @param $timeout
         * @param {Trace} trace
         * @param {AppEvents} appEvents
         */
        function ($interval, $timeout, trace, appEvents) {
            return {
                restrict: 'E',
                templateUrl: 'app/linkDetails/tabs/trace.html',
                link: function (scope, element, attrs) {
                    var tracingTimer;

                    scope.trace = {
                        min: 1,
                        max: 10,
                        minutes: 2,
                        isRunning: false
                    };

                    function reloadTracingData() {
                        var params = {
                            linkId: scope.link.id
                        };

                        trace.getConfigurations(params)
                            .then(function (data) {
                                scope.trace.logs = data;
                            });
                    }

                    function traceRuntimeFormat(startDate, endDate) {
                        var start = new Date(startDate).getTime();
                        var end = new Date(endDate).getTime();
                        var date = new Date(end - start);

                        return moment(date).format('mm:ss');
                    }

                    function updateTraceTimer() {
                        var startDate = Date.now();
                        var endDate = new Date(scope.trace.traceConfiguration.endDate);

                        var difference = endDate - startDate;

                        if (difference <= 0) {
                            stopTraceTimer();
                            reloadRunningTracingData();
                            return;
                        }

                        scope.trace.timeLeft = traceRuntimeFormat(startDate, endDate);
                    }

                    function startTraceTimer() {
                        if (angular.isDefined(tracingTimer)) {
                            return;
                        }

                        tracingTimer = $interval(updateTraceTimer, 1000);
                    }

                    function stopTraceTimer() {
                        if (!angular.isDefined(tracingTimer)) {
                            return;
                        }

                        $interval.cancel(tracingTimer);
                        tracingTimer = undefined;
                        scope.trace.timeLeft = null;
                    }

                    function reloadRunningTracingData() {
                        var params = {
                            linkId: scope.link.id
                        };

                        trace.isRunning(params)
                            .then(function (data) {
                                scope.trace.traceConfiguration = data.traceConfiguration;
                                scope.trace.isRunning = data.isRunning;

                                if (data.isRunning) {
                                    updateTraceTimer();
                                    startTraceTimer();
                                }
                            });
                    }

                    function startTracing() {
                        var data = {
                            linkId: scope.link.id,
                            minutes: scope.trace.minutes
                        };

                        trace.startTracing(data)
                            .then(function () {
                                reloadRunningTracingData();
                            });
                    }

                    function stopTracing() {
                        var data = {
                            traceConfigurationId: scope.trace.traceConfiguration.id
                        };

                        stopTraceTimer();
                        trace.stopTracing(data)
                            .then(function () {
                                reloadTracingData();
                                reloadRunningTracingData();
                            });
                    }

                    function getTraceById(traceId) {
                        var itemToReturn = null;

                        scope.traceResults.forEach(function (item) {
                            if (item.id === traceId) {
                                itemToReturn = item;
                            }
                        });

                        return itemToReturn;
                    }

                    function showTraceResult(traceId) {
                        var isShown = getTraceById(traceId) !== null;

                        if (isShown) {
                            // TODO: Needs refactoring for communication with linkTraceTab-directive
                            scope.setActiveTab(traceId);
                            return;
                        }

                        var params = {
                            traceConfigurationId: traceId
                        };

                        return trace.getConfiguration(params)
                            .then(function (data) {
                                scope.tabs.push({
                                    name: data.id,
                                });

                                // TODO: Needs refactoring for communication with linkTraceTab-directive
                                scope.traceResults.push(data);

                                $timeout(function(){
                                    // select tab
                                    scope.setActiveTab(data.id);
                                });
                            });
                    }

                    scope.$on('$destroy', function () {
                        stopTraceTimer();
                    });

                    scope.$on(appEvents.reloadTraces, function () {
                        reloadTracingData();
                        reloadRunningTracingData();
                    });

                    scope.startTracing = startTracing;
                    scope.stopTracing = stopTracing;
                    scope.traceRuntimeFormat = traceRuntimeFormat;
                    scope.showTraceResult = showTraceResult;

                    // TODO: Needs refactoring for communication with linkTraceResultTab-directive
                    scope.getTraceById = getTraceById;

                    // Reload traces, if scope.link changes and the current active tab is trace
                    // Happens, when the user reloads the app while viewing the traces
                    // "TabActivated" Broadcast will be executed before scope.$on in this directive is executed
                    var unwatch = scope.$watch('link', function (newVal) {
                        if (newVal && scope.activeTabIndex === 3) {
                            unwatch();
                            reloadTracingData();
                            reloadRunningTracingData();
                        }
                    });

                    // Open the trace result, if it was set
                    if (scope.openTraceId) {
                        showTraceResult(scope.openTraceId);
                        delete scope.openTraceId;
                    }
                }
            };
        });
}();
