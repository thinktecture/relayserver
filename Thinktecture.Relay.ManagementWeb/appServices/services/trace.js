(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function Trace($http, apiUrl) {
        var baseUrl = apiUrl + 'trace/';

        /**
         * @param data
         * @returns {Promise}
         */
        this.startTracing = function (data) {
            return $http.post(baseUrl + 'traceconfiguration', data);
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.stopTracing = function (params) {
            return $http.delete(baseUrl + 'traceconfiguration', { params: params });
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.getConfigurations = function (params) {
            return $http.get(baseUrl + 'traceconfigurations', { params: params });
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.getConfiguration = function (params) {
            return $http.get(baseUrl + 'traceconfiguration', { params: params });
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.getFileInformations = function (params) {
            return $http.get(baseUrl + 'fileinformations', { params: params });
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.isRunning = function (params) {
            return $http.get(baseUrl + 'isrunning', { params: params });
        };

        /**
         * @param {string} url
         * @returns {string}
         */
        this.getDownloadUrl = function (url) {
            return baseUrl + 'download?url=' + encodeURI(url);
        };

        /**
         * @param {string} url
         * @returns {Promise}
         */
        this.getFileContent = function (headerFileName) {
            return $http.get(baseUrl + 'view?headerFileName=' + encodeURI(headerFileName), {transformResponse: null});
        };
    }

    app.module.service('trace', Trace);
})();
