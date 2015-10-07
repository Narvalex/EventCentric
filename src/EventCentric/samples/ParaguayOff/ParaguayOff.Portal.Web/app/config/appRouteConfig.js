'use strict';

angular.module('app').config(['$routeProvider', function ($routeProvider) {

    var routes = [
        {
            url: '/home',
            config: {
                title: 'Atención al cliente',
                templateUrl: '/app/atencionAlCliente/atencionAlCliente.html'
            }
        },
            {
                url: '/atencion-al-cliente/cliente/:clienteId',
                config: {
                    title: 'Atención al cliente',
                    templateUrl: '/app/atencionAlCliente/formularioCliente.html'
                }
            },
        {
            url: '/forms',
            config: {
                title: 'Forms',
                template: '<h2>Forms</h2>'
            }
        },
        {
            url: '/lists',
            config: {
                title: 'Lists',
                template: '<h2>Lists</h2>'
            }
        }
    ];

    routes.forEach(function (route) {
        $routeProvider.when(route.url, route.config);
    });

    $routeProvider.otherwise({ redirectTo: '/home' });

}]);