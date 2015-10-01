'use strict';

angular.module('app').config(['$routeProvider', function ($routeProvider) {

    var routes = [
        {
            url: '/dashboard',
            config: {
                title: 'Dashboard',
                template: '<lx-dashboard></lx-dashboard>'
            }
        },
        {
            url: '/forms',
            config: {
                title: 'Forms',
                template: '<lx-forms></lx-forms>'
            }
        },
        {
            url: '/lists',
            config: {
                title: 'Lists',
                template: '<lx-lists></lx-lists>'
            }
        }
    ];

    routes.forEach(function (route) {
        $routeProvider.when(route.url, route.config);
    });

    $routeProvider.otherwise({ redirectTo: '/dashboard' });

}]);