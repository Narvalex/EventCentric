'use strict';

angular.module('lxMenu').directive('lxMenuItem', function () {
    return {
        require: '^lxMenu', // we require the controller from lxMenu
        scope: { // an isolate scope
            label: '@',
            icon: '@',
            route: '@'
        },
        templateUrl: 'modules/lxMenu/lxMenuItemTemplate.html',
        link: function (scope, el, attr, ctrl) { // we can access the controller with the link function

            scope.isActive = function () {
                return el === ctrl.getActiveItem();
            };

            el.on('click', function (event) { // subscribe to click event on this element
                event.stopPropagation(); // exclusive handling of the event
                event.preventDefault(); // prevents default behavior to ocurr
                scope.$apply(function () { // because this is hapening 'in JQuery' we need to tell angular that is happening something
                    ctrl.setActiveItem(el);
                    ctrl.setRoute(scope.route);
                });
            });
        }
    };
});