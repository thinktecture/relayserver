(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function Dashboard($http, apiUrl) {
        var baseUrl = apiUrl + 'dashboard/';

        /**
         * @returns {Promise}
         */
        this.info = function () {
            return $http.get(baseUrl + 'info');
        };
    }

    app.module.service('dashboard', Dashboard);
})();
