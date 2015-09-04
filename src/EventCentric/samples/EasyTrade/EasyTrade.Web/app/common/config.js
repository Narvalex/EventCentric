(function () {
    'use strict';
    var app = angular.module('app');

    var keyCodes = {
        backspace: 8,
        tab: 9,
        enter: 13,
        esc: 27,
        space: 32,
        pageup: 33,
        pagedown: 34,
        end: 35,
        home: 36,
        left: 37,
        up: 38,
        right: 39,
        down: 40,
        insert: 45,
        del: 46
    };

    var config = {
        keyCodes: keyCodes
    }

    // Injectamos config en los modules
    app.value('config', config);

    var AuthHttpResponseInterceptor = function ($q, $location) {
        return {
            response: function (response) {
                if (response.status === 401) {
                    console.log("Response 401");
                    toastr.warning("401 Acceso denegado. Inicie sesión primero para continuar.");
                }
                return response || $q.when(response);
            },
            responseError: function (rejection) {
                if (rejection.status === 401) {
                    console.log("Response Error 401", rejection);
                    toastr.warning("401 Acceso denegado. Inicie sesión primero para continuar.");

                    // Redirect examples...
                    //$location.path('/login').search('returnUrl', $location.path());
                    //location.href = 'Registracion/Login';

                    // Por fin :D
                    var returnUrl = location.pathname + location.hash;
                    var urlBuilder = $location.path('Usuarios/Login').search('returnUrl', returnUrl)
                    location.href = urlBuilder.$$url;
                }
                return $q.reject(rejection);
            }
        }
    };

    AuthHttpResponseInterceptor.$inject = ['$q', '$location'];

    app.factory('AuthHttpResponseInterceptor', AuthHttpResponseInterceptor);

})();