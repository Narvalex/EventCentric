'use strict';

angular.module('lxLayout').directive('lxLayout', function () {
    return {
        transclude: true,
        scope: { // isolate scope
            title: '@', // bind the string, one time
            subtitle: '@',
            iconFile: '@'
        },
        controller: 'lxLayoutController',
        templateUrl: 'external-modules/lxLayout/lxLayoutTemplate.html'
    };
});