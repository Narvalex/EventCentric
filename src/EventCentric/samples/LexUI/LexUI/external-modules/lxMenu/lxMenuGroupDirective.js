'use strict';

angular.module('lxMenu').directive('lxMenuGroup', function () {
    return {
        require: '^lxMenu',
        transclude: true,
        scope: {
            label: '@'
        },
        templateUrl: 'external-modules/lxMenu/lxMenuGroupTemplate.html',
        link: function (scope, el, attrs, ctrl) {
            scope.isOpen = false;
            scope.closeMenu = function () {
                scope.isOpen = false;
            };

            scope.clicked = function () {
                scope.isOpen = !scope.isOpen;
            }
        }
    };
});