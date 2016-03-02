(function ($, jQuery) {
    "use strict";

    /**
     * @param $scope
     * @param $state
     * @param $translate
     * @param {User} user
     * @param {NotificationService} notificationService
     * @constructor
     */
    function SetupController($scope, $state, $translate, user, notificationService) {
        $scope.createUser = function () {
            var data = {
                username: $scope.username,
                password: $scope.password
            };
            user.createFirstTimeUser(data)
                .then(function () {
                    $translate('SETUP.USER_CREATED')
                        .then(function (text) {
                            $state.go('login');
                            notificationService.success(text);
                        });
                }, function () {
                    $translate('SETUP.USER_NOT_CREATED')
                        .then(function (text) {
                            notificationService.error(text);
                        });
                });
        };

        $scope.$watch(function() {
           $scope.passwordsDoNotMatch = $scope.password != $scope.password2;
        });
    }

    app.module.controller('setupController', SetupController);
})();
