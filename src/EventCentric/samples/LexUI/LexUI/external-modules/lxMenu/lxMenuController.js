'use strict';

angular.module('lxMenu').controller('lxMenuController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {

            $scope.showMenu = false;

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

            $scope.$on('lxMessage-showMenuStateChanged', function (event, data) {
                $scope.showMenu = data.show;
            });
}]);