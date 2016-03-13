(function () {
    "use strict";

    app.module.config(function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider.otherwise('/');

        var linkDetailsState = createState('linkDetails');
        linkDetailsState.url = '/details?id&tab';
        linkDetailsState.views = {
            '@': {
                templateUrl: 'app/linkDetails/linkDetails.html',
                controller: 'linkDetailsController'
            }
        };

        $stateProvider
            .state('dashboard', createState('dashboard', false, '/'))
            .state('links', createState('links'))
            .state('links.details', linkDetailsState)
            .state('users', createState('users'))
            .state('login', createState('login', true, '/login?redirectTo'))
            .state('setup', createState('setup', true));
    });

    function createState(name, anonymous, url) {
        return {
            url: url || '/' + name,
            templateUrl: 'app/' + name + '/' + name + '.html',
            controller: name + 'Controller',
            data: {
                anonymous: !!anonymous
            }
        };
    }
})();
