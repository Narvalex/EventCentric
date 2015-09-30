'use strict';

angular.module('app').config(['$routeProvider', function ($routeProvider) {

    var routes = [
        {
            url: '/dashboard',
            config: {
                template: '<lx-dashboard></lx-dashboard>'
            }
        },
        {
            url: '/forms',
            config: {
                template: '<lx-forms></lx-forms>'
            }
        },
        {
            url: '/lists',
            config: {
                template: '<lx-lists></lx-lists>'
            }
        }
    ];

    routes.forEach(function (route) {
        $routeProvider.when(route.url, route.config);
    });

    $routeProvider.otherwise({ redirectTo: '/dashboard' });

}]);