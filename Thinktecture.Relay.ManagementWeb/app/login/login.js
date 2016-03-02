(function () {
    "use strict";

    /**
     * @param $scope
     * @param $state
     * @param $translate
     * @param {Security} security
     * @param {NotificationService} notificationService
     * @constructor
     */
    function LoginController($scope, $state, $stateParams, $translate, security, notificationService) {
        $scope.login = function() {
            security.login($scope.username, $scope.password, $scope.rememberMe)
                .then(function() {
                    if ($stateParams.redirectTo) {
                        return $state.go($stateParams.redirectTo);
                    }

                    $state.go('dashboard');
                }, function() {
                    $translate('LOGIN.FAILED')
                        .then(function (text) {
                            notificationService.error(text);
                        });
                });
        };
    }

    app.module.controller('loginController', LoginController);
})();
