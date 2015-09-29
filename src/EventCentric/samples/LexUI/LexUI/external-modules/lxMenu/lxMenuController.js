'use strict';

angular.module('lxMenu').controller('lxMenuController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {

            this.getActiveItem = function () {
                return $scope.activeElement;
            };

            this.setActiveItem = function (el) {
                $scope.activeElement = el;
            };

            this.setRoute = function (route) {
                $rootScope.$broadcast('lxMessage-ItemSelected',
                {
                    route: route
                });
        };
}]);