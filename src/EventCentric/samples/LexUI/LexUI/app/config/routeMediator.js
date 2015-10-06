'use strict';

angular.module('app').factory('routeMediator',
 ['$location', '$rootScope', 'config', 
     function ($location, $rootScope, config) {
         // Define the functions and properties to reveal.
         var handleRouteChangeError = false;

         return {
             setRoutingHandlers: setRoutingHandlers
         };

         function setRoutingHandlers() {
             updateDocTitle();
             handleRoutingErrors();
         }

         function updateDocTitle() {
             $rootScope.$on('$routeChangeSuccess',
                function (event, current, previous) {
                    handleRouteChangeError = false;
                    var title = config.docTitle + (current.title || '');
                    $rootScope.title = title;
                });
         }

         function handleRoutingErrors() {
             $rootScope.$on('$routeChangeError',
                 function (event, current, previous, rejection) {
                     if (handleRouteChangeError) { return; }
                     handleRouteChangeError = true;
                     var msg = 'Error routing: ' + (current && current.name)
                         + '. ' + (rejection.msg || '');

                     console.log(msg + ' ' + current);

                     // This makes an infinite loop....
                     //$location.path('/');
                 });
         }

     }
 ]);

