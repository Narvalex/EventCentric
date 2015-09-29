'use strict';

angular.module('lxMenu').controller('lxMenuController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {

            $scope.showMenu = false;
            $scope.isVertical = true;

            this.getActiveItem = function () {
                return $scope.activeElement;
            };

            this.setActiveItem = function (el) {
                $scope.activeElement = el;
            };

            this.isVertical = function () {
                return $scope.isVertical;
            };

            this.setRoute = function (route) {
                $rootScope.$broadcast('lxMessage-itemSelected',
                {
                    route: route
                });
            };

            $scope.$on('lxMessage-showMenuStateChanged', function (event, data) {
                $scope.showMenu = data.show;
            });

            $scope.toggleMenuOrientation = function () {
                $scope.isVertical = !$scope.isVertical;

                $rootScope.$broadcast('lxMessage-menuOrientationChanged',
                {
                    isMenuVertical: $scope.isVertical
                });
            };

}]);