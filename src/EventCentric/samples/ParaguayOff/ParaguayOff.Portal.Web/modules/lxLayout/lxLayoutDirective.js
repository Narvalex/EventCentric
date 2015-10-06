'use strict';

angular.module('lxLayout').directive('lxLayout', function () {
    return {
        transclude: true,
        scope: { // isolate scope
            appTitle: '@', // bind the string, one time
            appSubtitle: '@',
            iconFile: '@'
        },
        controller: 'lxLayoutController',
        templateUrl: 'modules/lxLayout/lxLayoutTemplate.html'
    };
});