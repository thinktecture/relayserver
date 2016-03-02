(function ($, jQuery) {
    "use strict";

    /**
     * @param $scope
     * @param $uibModal
     * @param $translate
     * @param $state
     * @param {Link} link
     * @param {NotificationService} notificationService
     * @param uiGridConstants
     * @param {SpinnerOverlay} spinnerOverlay
     * @constructor
     */
    function LinksController($scope, $uibModal, $translate, $state,
                             link, notificationService, uiGridConstants,
                             spinnerOverlay) {
        // Fields are the same as in WebAPI PageRequest
        var paging = {
            page: 1,
            pageSize: 10,
            maxItems: 0,
            searchText: "",
            sortField: "symbolicName",
            sortDirection: "ascending"
        };

        var pageSizes = [
            10,
            20,
            50,
            100
        ];

        $scope.paging = paging;
        $scope.pageSizes = pageSizes;

        $scope.gridOptions = {
            enableColumnMenu: false,
            useExternalSorting: true,
            minRowsToShow: 11,
            columnDefs: [
                {
                    displayName: $translate.instant('LINKS.SYMBOLIC_NAME'),
                    field: 'symbolicName',
                    width: '30%',
                    cellTemplate: 'app/links/linkTemplate.html',
                    sort: {
                        direction: uiGridConstants.ASC
                    }
                },
                {
                    displayName: $translate.instant('LINKS.USERNAME'),
                    field: 'userName',
                    width: '30%'
                },
                {
                    displayName: 'ID',
                    field: 'id',
                    width: '25%',
                },
                {
                    displayName: $translate.instant('LINKS.CREATION_DATE'),
                    field: 'creationDate',
                    cellFilter: 'date',
                    width: '10%'
                },
                {
                    headerCellTemplate: '<div class="ui-grid-cell-contents text-center"><i class="fa fa-flash"></i></div> ',
                    enableSorting: false,
                    field: 'linkStatus',
                    width: '5%',
                    cellTemplate: 'app/links/linkStatusCellTemplate.html'
                }
            ],
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
                $scope.gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
                    if (sortColumns.length === 0) {
                        $scope.paging.sortField = "symbolicName";
                        $scope.paging.sortDirection = "ascending";

                        reloadLinks();

                        return;
                    }

                    $scope.paging.sortDirection = uiGridConstants.ASC;

                    if (sortColumns[0].sort.direction === uiGridConstants.DESC) {
                        $scope.paging.sortDirection = uiGridConstants.DESC;
                    }

                    $scope.paging.sortField = sortColumns[0].field;

                    reloadLinks();
                });
            }
        };

        function reloadLinks() {
            spinnerOverlay.open();
            link.getLinks($scope.paging)
                .then(function (data) {
                    spinnerOverlay.close();
                    $scope.gridOptions.data = data.items;
                    $scope.paging.maxItems = data.count;
                }, function () {
                    spinnerOverlay.close();
                });
        }

        reloadLinks();

        $scope.pageChanged = function () {
            reloadLinks();
        };

        $scope.view = function (id) {
            $state.go('links.details', {
                id: id
            });
        };

        $scope.createLink = function () {
            var modal = $uibModal.open({
                templateUrl: 'app/links/createLinkModal.html',
                controller: 'createLinkModalController',
                backdrop: 'static'
            });

            modal.result
                .then(function () {
                    // TODO: Highlight/navigate to link?
                    $translate('LINKS.NOTIFICATIONS.CREATE_SUCCESS')
                        .then(function (text) {
                            notificationService.success(text);
                        });
                    reloadLinks();
                }, function (error) {
                    if (error !== 'cancel' && error !== 'backdrop click' && error !== 'escape key press') {
                        $translate('LINKS.NOTIFICATIONS.CREATE_ERROR')
                            .then(function (text) {
                                notificationService.error(text);
                            });
                    }
                });
        };
    }

    app.module.controller('linksController', LinksController);

    /**
     * @param $scope
     * @param $uibModalInstance
     * @param {Link} link
     * @param {SpinnerOverlay} spinnerOverlay
     * @constructor
     */
    function CreateLinkModalController($scope, $uibModalInstance, link, spinnerOverlay) {
        function simplifySymbolicName(symbolicName) {
            return symbolicName ? symbolicName.replace(/\s/g, '-').toLowerCase() : '';
        }

        $scope.link = {};
        $scope.userNameCheck = {};

        $scope.submit = function () {
            spinnerOverlay.open();
            link.create($scope.link)
                .then(function (result) {
                    spinnerOverlay.close();
                    $scope.password = result.password;
                }, function () {
                    spinnerOverlay.close();
                    $uibModalInstance.dismiss('error');
                });
        };

        $scope.closeSuccess = function () {
            $uibModalInstance.close();
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.$watch(function () {
            $scope.userNameUnavailable = !$scope.userNameCheck.available && $scope.link.userName !== undefined && $scope.userNameCheck.performed;
            $scope.userNameAvailable = $scope.userNameCheck.available && $scope.link.userName !== undefined;
        }, function (newVal, oldVal) {
        });

        $scope.$watch('link.symbolicName', function (newVal, oldVal) {
            if (!newVal) {
                $scope.link.userName = '';
            }

            if (newVal && newVal !== oldVal && $scope.createLinkForm.userName.$pristine) {
                $scope.link.userName = simplifySymbolicName($scope.link.symbolicName);
            }
        });

        $scope.$watch('link.userName', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                link.checkUserName(newVal)
                    .then(function (result) {
                        $scope.userNameCheck = result;
                    });
            }
        });
    }

    app.module.controller('createLinkModalController', CreateLinkModalController);
})();
