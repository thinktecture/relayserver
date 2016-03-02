(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function User($http, apiUrl) {
        var baseUrl = apiUrl + 'user/';

        /**
         * @returns {Promise}
         */
        this.getUsers = function () {
            return $http.get(baseUrl + 'users');
        };

        /**
         * @returns {Promise}
         */
        this.checkUserName = function(userName) {
            var checkResult = {
                performed: true,
                available: false
            };

            return $http.get(baseUrl + 'userNameAvailability', { params: { userName: userName } })
                .then(function() {
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
         * @returns {Promise}
         */
        this.create = function (user) {
            return $http.post(baseUrl + 'user', user);
        };

        /**
         * @returns {Promise}
         */
        this.update = function (user) {
            return $http.put(baseUrl + 'user', user);
        };

        /**
         * @returns {Promise}
         */
        this.createFirstTimeUser = function (user) {
            return $http.post(baseUrl + 'firsttime', user);
        };

        /**
         * @returns {Promise}
         */
        this.delete = function (id) {
            return $http.delete(baseUrl + 'user', {
                params: {
                    id: id
                }
            });
        };
    }

    app.module.service('user', User);
})();
