'use strict';

angular.module('lxMenu').directive('lxMenuGroup', function () {
    return {
        require: '^lxMenu',
        transclude: true,
        scope: {
            label: '@'
        },
        templateUrl: 'modules/lxMenu/lxMenuGroupTemplate.html',
        link: function (scope, el, attrs, ctrl) {
            scope.isOpen = false;
            scope.closeMenu = function () {
                scope.isOpen = false;
            };

            scope.clicked = function () {
                scope.isOpen = !scope.isOpen;

                if (el.parents('.lx-subitem-section').length == 0)
                    scope.setSubmenuPosition();

                ctrl.setOpenMenuScope(scope);
            }

            scope.setSubmenuPosition = function () {
                var pos = el.offset();
                $('.lx-subitem-section').css({ 'left': pos.left + 20, 'top': 36 });
            }
        }
    };
});