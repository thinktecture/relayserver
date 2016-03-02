(function ($, jQuery) {
    "use strict";

    /**
     * @param $scope
     * @param $state
     * @param {Security} security
     * @constructor
     */
    function LogoutController($scope, $state, security) {
        $scope.logout = function() {
            security.logout();
            $state.go('login');
        };

        $scope.isLoggedIn = security.isLoggedIn;
    }

    app.module.controller('logoutController', LogoutController);
})();
