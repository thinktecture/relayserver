!function ($, jQuery, window, document) {
    'use strict';

    app.module.directive('linkTraceResultTab',
        /**
         * @param {SpinnerOverlay} spinnerOverlay
         * @param {Trace} trace
         * @param {AppEvents} appEvents
         */
        function (spinnerOverlay, trace, appEvents) {
            return {
                restrict: 'E',
                templateUrl: 'app/linkDetails/tabs/traceResult.html',
                link: function (scope, element, attrs) {
                    function loadFileInformation(traceId) {
                        // TODO: Needs refactoring for communication with linkTraceTab-directive
                        var item = scope.getTraceById(traceId);

                        if (item === null) {
                            return;
                        }

                        if (item.logs) {
                            return;
                        }

                        var params = {
                            traceConfigurationId: item.id
                        };

                        spinnerOverlay.open();
                        trace.getFileInformations(params)
                            .then(function (data) {
                                item.logs = data;
                            })
                            .finally(function () {
                                spinnerOverlay.close();
                            });
                    }

                    function getDownloadUrl(url) {
                        return trace.getDownloadUrl(url);
                    }

                    function viewContentModal(urlToDownload) {
                        spinnerOverlay.open();
                        trace.getFileContent(urlToDownload)
                            .then(function (content) {
                                spinnerOverlay.close();
                                $uibModal.open({
                                    templateUrl: 'app/linkDetails/viewContentModal.html',
                                    controller: 'viewContentModalController',
                                    resolve: {
                                        content: function () {
                                            return content;
                                        }
                                    }
                                });
                            }, function () {
                                spinnerOverlay.close();
                            });
                    }

                    scope.getDownloadUrl = getDownloadUrl;
                    scope.viewContent = viewContentModal;

                    loadFileInformation(scope.trace.id);
                }
            };
        });

    /**
     * @param scope
     * @param $uibModalInstance
     * @param {string} content
     * @constructor
     */
    function ViewContentModalController(scope, $uibModalInstance, content) {
        scope.content = content;

        scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    }

    app.module.controller('viewContentModalController', ViewContentModalController);
}();
