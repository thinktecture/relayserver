(function ($, jQuery) {
    "use strict";

    /**
     * @returns {Function}
     * @constructor
     */
    function Filter() {
        /**
         * @param {*|object} value
         * @param {*|string} defaultText
         * @returns {*|object}
         */
        return function (value, defaultText) {
            if (value) {
                return value;
            }

            return defaultText;
        };
    }

    app.module.filter('defaultTextWhenFalsy', Filter);
})();
