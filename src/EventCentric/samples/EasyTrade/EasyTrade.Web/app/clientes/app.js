(function () {
    'use strict';

    var app = angular.module('app', [
        'ui.router',
    ]);

    app.config([
        '$stateProvider',
        '$urlRouterProvider',
        '$locationProvider',
        '$httpProvider',
        function ($stateProvider, $urlRouterProvider, $locationProvider, $httpProvider) {
            $urlRouterProvider.otherwise("/");

            $stateProvider
            .state("index", {
                url: "/",
                templateUrl: "/app/clientes/index.html"
            })
            .state("nuevoCliente", {
                url: "/nuevo-cliente",
                templateUrl: "/app/clientes/nuevoCliente.html"
            });

            $locationProvider.html5Mode(false);

            $httpProvider.interceptors.push('AuthHttpResponseInterceptor');
        }
    ]);

    // (to do stuff when an error occurs)
    app.config(function ($provide) {
        $provide.decorator("$exceptionHandler",
            ["$delegate",
                function ($delegate) {
                    return function (exception, cause) {
                        exception.message = "Fatal error: " + exception.message;
                        $delegate(exception, cause);

                        //alert(exception.message);
                        console.log(exception.message);
                    };
                }]);
    });

    // Kick start the router
    app.run(['$state',
        function ($state) {
            $state.go('index');

            configToastr();
        }]);

    function configToastr() {
        // More info: http://codeseven.github.io/toastr/demo.html
        toastr.options = {
            "closeButton": true,
            "debug": true,
            "positionClass": "toast-bottom-right",
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "10000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
    }

})();