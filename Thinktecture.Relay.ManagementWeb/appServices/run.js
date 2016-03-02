(function ($, jQuery) {
    "use strict";

    app.module.run(function ($rootScope, $state, security, setup) {
        setup.needsFirstTimeSetup()
            .catch(function (response) {
                if (response.status === 307) {
                    $state.go('setup');
                }
            });

        $rootScope.$on('$stateChangeStart', function (e, toState) {
            if (!security.isLoggedIn() && !toState.data.anonymous) {
                e.preventDefault();
                $state.go('login', {
                    redirectTo: toState.name
                });
            }
        });

        $rootScope.$on(security.events.needsAuthentication, function (e, args) {
            e.preventDefault();
            $state.go('login', args);
        });
    });
})();
