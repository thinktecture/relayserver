(function ($, jQuery) {
    "use strict";

    /**
     * @param $http
     * @param {string} apiUrl
     * @constructor
     * @public
     */
    function Setup($http, apiUrl) {
        /**
         * @returns {Promise}
         */
        this.needsFirstTimeSetup = function() {
            return $http.get(apiUrl + 'setup/needsfirsttimesetup');
        };
    }

    app.module.service('setup', Setup);
})();
