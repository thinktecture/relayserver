(function ($, jQuery) {
    "use strict";

    /**
     * @param $scope
     * @param $uibModal
     * @param $translate
     * @param {User} user
     * @param {NotificationService} notificationService
     * @param uiGridConstants
     * @constructor
     */
    function UsersController($scope, $uibModal, $translate, user, notificationService, uiGridConstants) {
        $scope.gridOptions = {
            enableColumnMenu: false,
            columnDefs: [
                {
                    displayName: $translate.instant('COMMON.USERNAME'),
                    field: 'userName',
                    width: '80%',
                    sort: {
                        direction: uiGridConstants.ASC
                    }
                },
                {
                    displayName: $translate.instant('USERS.OPTIONS'),
                    name: 'options',
                    cellTemplate: 'app/users/userOptionsCellTemplate.html',
                    width: '20%'
                }
            ]
        };

        function reloadUsers() {
            user.getUsers()
                .then(function (data) {
                    $scope.gridOptions.data = data;
                });
        }

        reloadUsers();

        $scope.createUser = function () {
            var modal = $uibModal.open({
                templateUrl: 'app/users/createUserModal.html',
                controller: 'createUserModalController',
                resolve: {
                    affectedUser: function () { return null; }
                }
            });

            modal.result
                .then(function (userToAdd) {
                    return user.create(userToAdd);
                })
                .then(function (userId) {
                    // TODO: Highlight/navigate to user?
                    $translate('USERS.NOTIFICATIONS.CREATE_SUCCESS')
                        .then(function (text) {
                            notificationService.success(text);
                        });
                    reloadUsers();
                }, function (error) {
                    if (error !== 'cancel' && error !== 'backdrop click' && error !== 'escape key press') {
                        $translate('USERS.NOTIFICATIONS.CREATE_ERROR')
                            .then(function (text) {
                                notificationService.error(text);
                            });
                    }
                });
        };

        function deleteUser (userToDelete) {
            var modal = $uibModal.open({
                templateUrl: 'app/users/deleteUserModal.html',
                controller: 'deleteUserModalController',
                resolve: {
                    affectedUser: function () { return userToDelete; }
                }
            });

            modal.result
                .then(function () {
                    return user.delete(userToDelete.id);
                })
                .then(function () {
                    $translate('USERS.NOTIFICATIONS.DELETE_SUCCESS')
                        .then(function (text) {
                            notificationService.success(text);
                        });
                    reloadUsers();
                }, function (error) {
                    if (error !== 'cancel' && error !== 'backdrop click' && error !== 'escape key press') {
                        $translate('USERS.NOTIFICATIONS.DELETE_ERROR')
                            .then(function (text) {
                                notificationService.error(text);
                            });
                    }
                });
        }

        function editPasswordForUser (userToEdit) {
            var modal = $uibModal.open({
                templateUrl: 'app/users/createUserModal.html',
                controller: 'createUserModalController',
                resolve: {
                    affectedUser: function () { return userToEdit; }
                }
            });

            modal.result
                .then(function (updateUser) {
                    return user.update(updateUser);
                })
                .then(function () {
                    $translate('USERS.NOTIFICATIONS.EDIT_PASSWORD_SUCCESS')
                        .then(function (text) {
                            notificationService.success(text);
                        });
                    reloadUsers();
                }, function (error) {
                    if (error !== 'cancel' && error !== 'backdrop click' && error !== 'escape key press') {
                        $translate('USERS.NOTIFICATIONS.EDIT_PASSWORD_ERROR')
                            .then(function (text) {
                                notificationService.error(text);
                            });
                    }
                });
        }

        $scope.externalScope = {
            deleteUser: function (user) {
                deleteUser(user);
            },
            editPasswordForUser: function (user) {
                editPasswordForUser(user);
            }
        };
    }

    app.module.controller('usersController', UsersController);

    /**
     * @param $scope
     * @param $uibModalInstance
     * @param affectedUser
     * @constructor
     */
    function CreateUserModalController($scope, $uibModalInstance, user, affectedUser) {
        $scope.user = {};
        $scope.userNameCheck = {};

        if (affectedUser) {
            $scope.isEditMode = true;
            $scope.user = affectedUser;
            $scope.userNameUnavailable = false;
        }

        $scope.$watch(function () {
            $scope.userNameUnavailable = !$scope.userNameCheck.available && $scope.user.userName !== undefined && $scope.userNameCheck.performed;
            $scope.userNameAvailable = $scope.userNameCheck.available && $scope.user.userName !== undefined;
            $scope.passwordsDoNotMatch = $scope.user.password !== $scope.user.password2;
        }, function (newVar, oldVar) {
        });

        $scope.$watch('user.userName', function (newVar, oldVar) {
            if (newVar !== oldVar) {
                user.checkUserName(newVar)
                    .then(function (result) {
                        $scope.userNameCheck = result;
                    });
            }
        });

        $scope.submit = function () {
            $uibModalInstance.close($scope.user);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    }

    app.module.controller('createUserModalController', CreateUserModalController);

    /**
     * @param $scope
     * @param $uibModalInstance
     * @param affectedUser
     * @constructor
     */
    function DeleteUserModalController($scope, $uibModalInstance, affectedUser) {
        $scope.user = affectedUser;

        $scope.submit = function () {
            $uibModalInstance.close(affectedUser);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    }

    app.module.controller('deleteUserModalController', DeleteUserModalController);
})();
