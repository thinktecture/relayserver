(function () {
    "use strict";

    /**
     * @param $scope
     * @param $state
     * @param {Dashboard} dashboard
     * @param {SpinnerOverlay} spinnerOverlay
     * @constructor
     */
    function DashboardController($scope, $state, dashboard, spinnerOverlay) {
        // This code is copied from linkDetails.js
        // Should be placed in a directive
        $scope.chart = {
            options: {
                scaleLabel: "<%= Math.round((Number(value) / 1024)) + ' kB ' %>"
            },
            // Init with empty data array to prevent an error within angular-chart
            data: [
                []
            ],
            labels: []
        };

        function loadDashboard() {
            spinnerOverlay.open();
            dashboard.info()
                .then(function (data) {
                    $scope.dashboard = data;

                    var chartData = [
                        [],
                        []
                    ];
                    var chartLabels = [];

                    data.contentBytesChartDataItems.forEach(function (item) {
                        chartLabels.push(moment(item.key).format('YYYY-MM-DD'));
                        chartData[0].push(item.in);
                        chartData[1].push(item.out);
                    });

                    $scope.chart.labels = chartLabels;
                    $scope.chart.data = chartData;

                    spinnerOverlay.close();
                }, function () {
                    spinnerOverlay.close();
                });
        }

        loadDashboard();

        $scope.open = function (id) {
            $state.go('links.details', {
                id: id
            });
        };
    }

    app.module.controller('dashboardController', DashboardController);
})();
