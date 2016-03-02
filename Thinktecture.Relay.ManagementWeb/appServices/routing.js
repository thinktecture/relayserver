(function () {
    "use strict";

    app.module.config(function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider.otherwise('/dashboard');

        var linkDetailsState = createState('linkDetails');
        linkDetailsState.url = '/details?id&tab';
        linkDetailsState.views = {
            '@': {
                templateUrl: 'app/linkDetails/linkDetails.html',
                controller: 'linkDetailsController'
            }
        };

        var loginState = createState('login', true);
        loginState.url += '?redirectTo';

        $stateProvider
            .state('dashboard', createState('dashboard'))
            .state('links', createState('links'))
            .state('links.details', linkDetailsState)
            .state('users', createState('users'))
            .state('login', loginState)
            .state('setup', createState('setup', true));
    });

    function createState(name, anonymous) {
        return {
            url: '/' + name,
            templateUrl: 'app/' + name + '/' + name + '.html',
            controller: name + 'Controller',
            data: {
                anonymous: !!anonymous
            }
        };
    }
})();
