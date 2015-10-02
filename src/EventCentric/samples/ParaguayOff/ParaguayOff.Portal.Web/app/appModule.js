(function () {
    'use strict';

    var app = angular.module('app', [
        'lxLayout',         // Lex Layout
        'ngRoute',          // Routing

        // 3rd party modules
    ]);

    app.run(['$route', 'routeMediator',
        function ($route, routeMediator) {
            // Include $route to kick start the router.
            routeMediator.setRoutingHandlers();
        }
    ]);

})();