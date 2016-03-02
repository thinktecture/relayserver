(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function Log($http, apiUrl) {
        var baseUrl = apiUrl + 'log/';

        /**
         * @param link
         * @returns {Promise}
         */
        this.getLatestLogs = function (link) {
            return $http.get(baseUrl + 'recentLog', { params: { id: link.id }});
        };

        /**
         *
         * @param params
         * @returns {Promise}
         */
        this.getContentBytesChartData = function (params) {
            return $http.get(baseUrl + 'chartcontentbytes', { params: params });
        };
    }

    app.module.service('log', Log);
})();
