(function ($, jQuery) {
    "use strict";

    var escKeyCode = 27;

    app.module.directive('inplaceEdit', function () {
        return {
            restrict: 'E',
            scope: {
                value: '=',
                update: '&',
                errorCheck: '&',
                errorText: '@',
                hasError: '='
            },
            templateUrl: 'appServices/directives/inplaceEdit.html',
            link: function (scope, element) {
                scope.inEditMode = false;

                var reset = function () {
                    scope.internalValue = scope.value;
                    scope.inEditMode = false;
                };

                scope.$watch('value', function (newVar, oldVar) {
                    if (newVar !== oldVar) {
                        scope.internalValue = newVar;
                    }
                });

                scope.$watch('internalValue', function (newVar, oldVar) {
                    if (newVar !== oldVar) {
                        var errorHandler = scope.errorCheck();

                        if (errorHandler) {
                            errorHandler(newVar);
                        }
                    }
                });

                element.bind('keydown', function (event) {
                    if (event.which === escKeyCode) {
                        reset();
                        scope.$digest();
                    }
                });

                scope.submit = function () {
                    var updateHandler = scope.update();
                    updateHandler(scope.internalValue);
                    scope.inEditMode = false;
                };

                scope.reset = reset;

                if (scope.value) {
                    scope.internalValue = scope.value;
                }
            }
        };
    });
})();
