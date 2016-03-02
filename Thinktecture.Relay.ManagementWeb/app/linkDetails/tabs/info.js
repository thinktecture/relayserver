!function ($, jQuery, window, document) {
    'use strict';

    app.module.directive('linkInfoTab',
        /**
         * @param $uibModal
         * @param $translate
         * @param {NotificationService} notificationService
         * @param {Link} link
         */
        function ($uibModal, $translate, notificationService, link) {
            return {
                restrict: 'E',
                scope: true,
                templateUrl: 'app/linkDetails/tabs/info.html',
                link: function (scope) {
                    scope.errors = {};

                    function put(data, showNotification) {
                        link.update(data)
                            .then(function () {
                                if (showNotification) {
                                    $translate('LINKS.NOTIFICATIONS.LINK_UPDATED_SUCCESS', { linkName: scope.link.symbolicName })
                                        .then(function (text) {
                                            notificationService.success(text);
                                        });
                                }

                                scope.link = data;
                            }, function () {
                                $translate('LINKS.NOTIFICATIONS.LINK_UPDATED_ERROR', { linkName: scope.link.symbolicName })
                                    .then(function (text) {
                                        notificationService.error(text);
                                    });
                            });

                    }

                    function toggleDisabledState() {
                        var data = angular.copy(scope.link);
                        data.isDisabled = !data.isDisabled;
                        put(data);
                    }

                    function toggleForwardOnPremiseTargetErrorResponse() {
                        var data = angular.copy(scope.link);
                        data.forwardOnPremiseTargetErrorResponse = !data.forwardOnPremiseTargetErrorResponse;
                        put(data);
                    }

                    function toggleAllowLocalClientRequestsOnly() {
                        var data = angular.copy(scope.link);
                        data.allowLocalClientRequestsOnly = !data.allowLocalClientRequestsOnly;
                        put(data);
                    }

                    function updateUserName(userName) {
                        var data = angular.copy(scope.link);
                        data.userName = userName;
                        put(data, true);
                    }

                    function updateSymbolicName(symbolicName) {
                        var data = angular.copy(scope.link);
                        data.symbolicName = symbolicName;
                        put(data, true);
                    }

                    function confirmDelete() {
                        var modal = $uibModal.open({
                            templateUrl: 'app/linkDetails/deleteModal.html',
                            controller: 'deleteLinkModalController',
                            scope: scope
                        });

                        modal.result
                            .catch(function (error) {
                                if (error !== 'cancel' && error !== 'backdrop click' && error !== 'escape key press') {
                                    $translate('LINK_DETAILS.DELETE_UNSUCCESSFUL')
                                        .then(function (text) {
                                            notificationService.error(text);
                                        });
                                }
                            });
                    }

                    function isUserNameAvailable(userName) {
                        if (userName === scope.link.userName) {
                            scope.errors.userName = false;
                            return;
                        }

                        if (!userName) {
                            scope.errors.userName = true;
                            return;
                        }

                        link.checkUserName(userName)
                            .then(function (result) {
                                scope.errors.userName = result.performed && !result.available;
                            });
                    }

                    function pingLink() {
                        var scopeLink = scope.link;

                        scope.isPinging = true;

                        $translate('LINK_DETAILS.NOTIFICATIONS.PING_LINK', scopeLink)
                            .then(function (text) {
                                notificationService.info(text);
                            });

                        link.ping(scopeLink)
                            .then(function () {
                                scope.isPinging = false;
                                $translate('LINK_DETAILS.NOTIFICATIONS.PING_SUCCESS', scopeLink)
                                    .then(function (text) {
                                        notificationService.success(text);
                                    });
                            }, function () {
                                scope.isPinging = false;
                                $translate('LINK_DETAILS.NOTIFICATIONS.PING_ERROR', scopeLink)
                                    .then(function (text) {
                                        notificationService.error(text);
                                    });
                            });
                    }

                    scope.toggleDisabledState = toggleDisabledState;
                    scope.toggleForwardOnPremiseTargetErrorResponse = toggleForwardOnPremiseTargetErrorResponse;
                    scope.toggleAllowLocalClientRequestsOnly = toggleAllowLocalClientRequestsOnly;
                    scope.updateUserName = updateUserName;
                    scope.updateSymbolicName = updateSymbolicName;
                    scope.confirmDelete = confirmDelete;
                    scope.pingLink = pingLink;
                    scope.isUserNameAvailable = isUserNameAvailable;
                }
            };
        });

    /**
     * @param $scope
     * @param $uibModalInstance
     * @param $state
     * @param $translate
     * @param {NotificationService} notificationService
     * @param {Link} link
     * @constructor
     */
    function DeleteLinkModalController($scope, $uibModalInstance, $state, $translate, notificationService, link) {
        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.submit = function () {
            link.delete({ id: $scope.link.id })
                .then(function () {
                    $uibModalInstance.close();
                    $state.go('links');
                    $translate('LINK_DETAILS.DELETE_SUCCESS')
                        .then(function (text) {
                            notificationService.success(text);
                        });
                });
        };
    }

    app.module.controller('deleteLinkModalController', DeleteLinkModalController);
}();
