'use strict';

angular.module('lxLayout').controller('lxLayoutController',
    ['$scope', '$window', '$timeout',
        function ($scope, $window, $timeout) {

            $scope.isMenuVisible = true;
            $scope.isMenuButtonVisible = true;

            $scope.$on('lxMessage-ItemSelected',
                function (event, data) {
                    $scope.routeString = data.route;
                });

            $($window).on('resize.lxLayout', function () {
                $scope.$apply(function () {
                    checkWidth();
                });
            });

            $scope.$on('$destroy', function () {
                $($window).off('resize.lxLayout'); // remove the handler added earlier
            });

            $timeout(function () {
                checkWidth();
            }, 0);

            function checkWidth() {
                var width = Math.max($($window).width(), $window.innerWidth);
                $scope.isMenuVisible = (width >= 768);
                $scope.isMenuButtonVisible = !$scope.isMenuVisible;
            }
        }
    ]);