(function ($, jQuery) {
    "use strict";

    function isApiRequest(apiUrl, config) {
        return config && config.url && config.url.substr(0, apiUrl.length) === apiUrl;
    }

    /**
     * @param {string} apiUrl
     * @param {number} apiTimeout
     * @constructor
     */
    function ConfigInterceptor(apiUrl, apiTimeout) {
        return {
            request: function (config) {
                config = config || {};

                if (isApiRequest(apiUrl, config)) {
                    console.log('Api request: ' + config.url);

                    if (!angular.isDefined(config.timeout)) {
                        config.timeout = apiTimeout;
                    }
                }

                return config;
            }
        };
    }

    app.module.factory('configInterceptor', ConfigInterceptor);

    /**
     * @param $q
     * @param {string} apiUrl
     * @constructor
     */
    function ResponseInterceptor($q, apiUrl) {
        return {
            response: function (response) {
                if (isApiRequest(apiUrl, response.config)) {
                    return response.data;
                }

                return response || $q.when(response);
            }
        };
    }

    app.module.factory('responseInterceptor', ResponseInterceptor);

    /**
     * @constructor
     * @param $injector
     * @param $q
     */
    function TokenInterceptor($injector, $q) {
        function getSecurityService() {
            return $injector.get('security');
        }
        return {
            request: function (config) {
                var security = getSecurityService();
                var token = security.getToken();

                if (token) {
                    config.headers['Authorization'] = 'Bearer ' + token;
                }

                return config;
            },
            responseError: function (response) {
                if (response.status === 401) {
                    var security = getSecurityService();
                    security.logout(true);
                }


                return $q.reject(response);
            }
        };
    }

    app.module.factory('tokenInterceptor', TokenInterceptor);

    app.module.config(function ($httpProvider) {
        $httpProvider.interceptors.push('configInterceptor');
        $httpProvider.interceptors.push('responseInterceptor');
        $httpProvider.interceptors.push('tokenInterceptor');
    });
})();
