(function () {
    "use strict";

    /**
     * @param $rootScope
     * @param $http
     * @param $q
     * @param {string} apiHost
     * @constructor
     * @public
     */
    function Security($rootScope, $http, $q, $state, apiHost) {
        var storageKeys = {
            token: 'relay.token',
            user: 'relay.user'
        };

        var loginPromise, token, user;

        this.events = {
            needsAuthentication: 'needsAuthentication'
        };

        this.isLoggedIn = function () {
            return !!(user && token);
        };

        this.login = function (username, password, rememberMe) {
            if (loginPromise) {
                return loginPromise;
            }

            return loginPromise = $http.post(apiHost + 'token', {
                    'grant_type': 'password',
                    username: username,
                    password: password
                }, {
                    transformRequest: function (obj) {
                        var str = [];
                        for (var p in obj)
                            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                        return str.join("&");
                    },
                    headers: { "Content-Type": "application/x-www-form-urlencoded" }
                })
                .then(function (response) {
                    // TODO: Token Validation
                    if (response.data && response.data.access_token) {
                        saveUserData(response.data.access_token, username, rememberMe);
                        return;
                    }

                    return $q.reject('Token could not be obtained.');
                })
                .finally(function () {
                    loginPromise = undefined;
                });
        };

        this.getUser = function () {
            return user;
        };

        this.getToken = function () {
            return token;
        };

        function saveUserData(accessToken, username, rememberMe) {
            token = accessToken;
            user = username;

            var storage = rememberMe ? localStorage : sessionStorage;

            storage.setItem(storageKeys.token, token);
            storage.setItem(storageKeys.user, user);
        }

        this.logout = function (tryRedirectToCurrentState) {
            token = user = '';

            localStorage.removeItem(storageKeys.token);
            localStorage.removeItem(storageKeys.user);
            sessionStorage.removeItem(storageKeys.token);
            sessionStorage.removeItem(storageKeys.user);

            var args;

            if (tryRedirectToCurrentState) {
                args.redirectTo = $state.current.name;
            }

            $rootScope.$broadcast(this.events.needsAuthentication, args);
        };

        function activateLastActiveSession() {
            var cachedToken = localStorage.getItem(storageKeys.token) || sessionStorage.getItem(storageKeys.token);
            var cachedUser= localStorage.getItem(storageKeys.user) || sessionStorage.getItem(storageKeys.user);

            if (cachedToken && cachedToken !== null) {
                token = cachedToken;
            }

            if (cachedUser && cachedUser !== null) {
                user = cachedUser;
            }
        }

        activateLastActiveSession();
    }

    app.module.service('security', Security);
})();
