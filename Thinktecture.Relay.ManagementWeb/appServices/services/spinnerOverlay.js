(function ($, jQuery) {
    "use strict";

    /**
     * @param $rootScope
     * @param $uibModal
     * @param $timeout
     * @constructor
     * @public
     */
    function SpinnerOverlay($rootScope, $uibModal, $timeout) {
        // From: http://tobiasahlin.com/spinkit/
        var modalTemplate = '<div class="sk-fading-circle">' +
            '   <div class="sk-circle1 sk-circle"></div>' +
            '   <div class="sk-circle2 sk-circle"></div>' +
            '   <div class="sk-circle3 sk-circle"></div>' +
            '   <div class="sk-circle4 sk-circle"></div>' +
            '   <div class="sk-circle5 sk-circle"></div>' +
            '   <div class="sk-circle6 sk-circle"></div>' +
            '   <div class="sk-circle7 sk-circle"></div>' +
            '   <div class="sk-circle8 sk-circle"></div>' +
            '   <div class="sk-circle9 sk-circle"></div>' +
            '   <div class="sk-circle10 sk-circle"></div>' +
            '   <div class="sk-circle11 sk-circle"></div>' +
            '   <div class="sk-circle12 sk-circle"></div>' +
            '</div>';

        var modalInstance;
        var isOpen;
        var openPromise;
        var openDelay = 300;

        this.open = function () {
            if (!isOpen) {
                openPromise = $timeout(function() {
                    isOpen = true;
                    modalInstance = $uibModal.open({
                        template: modalTemplate,
                        backdrop: 'static',
                        windowClass: 'spinner-overlay'
                    });
                }, openDelay);
            }
        };

        this.close = function () {
            if (openPromise) {
                $timeout.cancel(openPromise);
            }

            if (modalInstance && isOpen) {
                modalInstance.close();
                isOpen = false;
            }
        };

        $rootScope.$on('$destroy', function () {
           if (openPromise) {
               $timeout.cancel(openPromise);
           }
        });
    }

    app.module.service('spinnerOverlay', SpinnerOverlay);
})();
