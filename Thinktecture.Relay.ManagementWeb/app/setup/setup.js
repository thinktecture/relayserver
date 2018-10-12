(function($, jQuery) {
  'use strict';

  /**
   * @param $scope
   * @param $state
   * @param $translate
   * @param {User} user
   * @param {NotificationService} notificationService
   * @constructor
   */
  function SetupController($scope, $state, $translate, user, notificationService) {
    $scope.createUser = function() {
      var data = {
        username: $scope.username,
        password: $scope.password,
        passwordVerification: $scope.passwordVerification,
      };
      user.createFirstTimeUser(data).then(
        function() {
          $translate('SETUP.USER_CREATED').then(function(text) {
            $state.go('login');
            notificationService.success(text);
          });
        },
        function(error) {
          var details = '';
          if (error.data && error.data.message) {
            details = '\r\n' + error.data.message;
          }

          $translate('SETUP.USER_NOT_CREATED').then(function(text) {
            notificationService.error(text + details);
          });
        }
      );
    };

    $scope.$watch(function() {
      $scope.passwordsDoNotMatch = $scope.password != $scope.passwordVerification;
    });
  }

  app.module.controller('setupController', SetupController);
})();
