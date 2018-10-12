!function ($, jQuery, window, document) {
    'use strict';

    app.module.directive('linkChartsTab',
        /**
         * @param $translate
         * @param {AppEvents} appEvents
         * @param {SpinnerOverlay} spinnerOverlay
         * @param {Log} log
         */
        function ($translate, appEvents, spinnerOverlay, log) {
            return {
                restrict: 'E',
                templateUrl: 'app/linkDetails/tabs/charts.html',
                link: function (scope) {
                    scope.dateFormat = 'YYYY-MM-DD';
                    var currentDate = new Date();

                    scope.isChartReloading = false;

                    scope.chart = {
                        options: {
                            scaleLabel: "<%= Math.round((Number(value) / 1024)) + ' kB ' %>"
                        },
                        // Init with empty data array to prevent an error within angular-chart
                        data: [
                            []
                        ],
                        labels: [],
                        fromDate: moment(currentDate).subtract(7, 'days').toDate(),
                        toDate: moment(currentDate).toDate(),
                        resolution: 'Daily',
                        resolutions: [
                            'Daily',
                            'Monthly',
                            'Yearly'
                        ]
                    };

                    $translate(['LINK_DETAILS.SERIES.CONTENT_IN', 'LINK_DETAILS.SERIES.CONTENT_OUT'])
                        .then(function (translations) {
                            scope.chart.series = [
                                translations['LINK_DETAILS.SERIES.CONTENT_IN'],
                                translations['LINK_DETAILS.SERIES.CONTENT_OUT']
                            ];
                        });

                    function reloadChart() {
                        scope.isChartReloading = true;
                        spinnerOverlay.open();

                        var data = {
                            id: scope.link.id,
                            start: moment(scope.chart.fromDate).format(scope.dateFormat),
                            end: moment(scope.chart.toDate).format(scope.dateFormat),
                            resolution: scope.chart.resolution
                        };

                        log.getContentBytesChartData(data)
                            .then(function (result) {
                                var chartData = [
                                    [],
                                    []
                                ];
                                var chartLabels = [];

                                result.forEach(function (item) {
                                    chartLabels.push(moment(item.key).format(scope.dateFormat));
                                    chartData[0].push(item.in);
                                    chartData[1].push(item.out);
                                });

                                scope.chart.labels = chartLabels;
                                scope.chart.data = chartData;
                            })
                            .finally(function () {
                                spinnerOverlay.close();
                                scope.isChartReloading = false;
                            });
                    }

                    function open(event) {
                        event.preventDefault();
                        event.stopPropagation();
                    }

                    function openFrom(event) {
                        open(event);

                        scope.chart.isFromDatePickerOpen = true;
                    }

                    function openTo(event) {
                        open(event);

                        scope.chart.isToDatePickerOpen = true;
                    }

                    scope.$on(appEvents.reloadChart, reloadChart);

                    scope.openFrom = openFrom;
                    scope.openTo = openTo;
                    scope.reloadChart = reloadChart;

                    // Reload chart, if scope.link changes and the current active tab is charts
                    // Happens, when the user reloads the app while viewing the chart
                    // "TabActivated" Broadcast will be executed before scope.$on in this directive is executed
                    var unwatch = scope.$watch('link', function (newVal) {
                        if (newVal && scope.activeTabIndex === 1) {
                            unwatch();
                            reloadChart();
                        }
                    });
                }
            };
        });
}();
