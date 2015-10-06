(function () {
    'use strict';

    var app = angular.module('app', [
        // Lex UI
        'lxLayout',         // Layout
        'ngRoute'           // Routing

        // 3rd Party Modules
    ]);

    app.run(['$route', 'routeMediator',
        function ($route, routeMediator) {
            // Include $route to kick start the router.
            routeMediator.setRoutingHandlers();
        }]);
})();