'use strict';

angular.module('lxMenu').directive('lxMenu', ['$timeout', function ($timeout) {
    return {
        scope: {

        },
        transclude: true,
        templateUrl: 'modules/lxMenu/lxMenuTemplate.html',
        controller: 'lxMenuController',
        link: function (scope, el, attr) {
            //  selecting the first item in the menu to bootstrap the app
            //var item = el.find('.lx-selectable-item:first');
            //$timeout(function () {
            //    item.trigger('click');
            //});
        }
    };
}]);