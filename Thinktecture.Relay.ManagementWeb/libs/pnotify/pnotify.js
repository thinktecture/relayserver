(function () {
    'use strict';

    /**
     * @param settings
     * @constructor
     * @public
     */
    function NotificationService(settings) {
        settings = angular.copy(settings);

        /* ========== SETTINGS RELATED METHODS =============*/

        this.getSettings = function () {
            return settings;
        };

        /* ============== NOTIFICATION METHODS ==============*/

        this.notice = function (content) {
            var hash = angular.copy(settings);
            hash.type = 'notice';
            hash.text = content;
            return this.notify(hash);
        };

        this.info = function (content) {
            var hash = angular.copy(settings);
            hash.type = 'info';
            hash.text = content;
            return this.notify(hash);
        };

        this.success = function (content) {
            var hash = angular.copy(settings);
            hash.type = 'success';
            hash.text = content;
            return this.notify(hash);
        };

        this.error = function (content) {
            var hash = angular.copy(settings);
            hash.type = 'error';
            hash.text = content;
            return this.notify(hash);
        };

        this.notify = function (hash) {
            return new PNotify(hash);
        };
    }

    /**
     * @constructor
     */
    function NotificationServiceProvider() {

        var settings = {
            styling: 'bootstrap3'
        };

        this.setDefaults = function (defaults) {
            settings = defaults
        };

        this.$get = [ function () {
            return new NotificationService(settings);
        }];

    }

    angular.module('ui.notify', []).
        provider('notificationService', [NotificationServiceProvider]);
})();
