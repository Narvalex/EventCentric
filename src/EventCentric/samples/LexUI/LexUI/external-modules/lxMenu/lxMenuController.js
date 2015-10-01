'use strict';

angular.module('lxMenu').controller('lxMenuController',
    ['$scope', '$rootScope',
        function ($scope, $rootScope) {

            $scope.isVertical = true;
            $scope.allowHorizontalToggle = true;

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
           

            this.setOpenMenuScope = function (scope) {
                $scope.openMenuScope = scope;
            }

            $scope.$on('lxMessage-showMenuStateChanged', function (event, data) {
                $scope.showMenu = data.show;
                $scope.isVertical = data.isVertical;
                $scope.allowHorizontalToggle = data.allowHorizontalToggle;
            });
        }
    ]);