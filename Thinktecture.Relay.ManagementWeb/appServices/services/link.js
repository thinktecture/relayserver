(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function Link($http, apiUrl) {
        var baseUrl = apiUrl + 'link/';

        /**
         * @returns {Promise}
         */
        this.getLinks = function (params) {
            return $http.get(baseUrl + 'links', { params: params });
        };

        this.getLink = function (params) {
            return $http.get(baseUrl + 'link', { params: params });
        };

        /**
         * @param data
         * @returns {Promise}
         */
        this.update = function (data) {
            return $http.put(baseUrl + 'link', data);
        };

        /**
         * @returns {Promise}
         */
        this.checkUserName = function (userName) {
            var checkResult = {
                performed: true,
                available: false
            };

            return $http.get(baseUrl + 'userNameAvailability', { params: { userName: userName } })
                .then(function () {
                    checkResult.available = true;

                    return checkResult;
                }, function (response) {
                    if (response.status === 0) {
                        checkResult.performed = false;
                    }

                    return checkResult;
                });
        };

        /**
         * @param link
         * @returns {Promise}
         */
        this.create = function (link) {
            return $http.post(baseUrl + 'link', link);
        };

        /**
         * @param link
         * @returns {Promise}
         */
        this.state = function (link) {
            return $http.put(baseUrl + 'state', link);
        };

        /**
         * @param link
         * @returns {Promise}
         */
        this.ping = function (link) {
            return $http.get(baseUrl + 'ping', { params: { id: link.id }});
        };

        /**
         * @param params
         * @returns {Promise}
         */
        this.delete = function (params) {
            return $http.delete(baseUrl + 'link', { params: params });
        };
    }

    app.module.service('link', Link);
})();
