'use strict';

angular.module('lxMenu').directive('lxMenu', function () {
    return {
        scope: {

        },
        transclude: true,
        templateUrl: 'external-modules/lxMenu/lxMenuTemplate.html',
        controller: 'lxMenuController',
        link: function (scope, el, attr) {

        }
    };
});